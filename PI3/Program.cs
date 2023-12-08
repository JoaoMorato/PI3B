using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PI3;
using System.Diagnostics;
using System.Text;

public class Program {

    private static List<int> Visitados = new List<int>();
    private static List<int> Caminho = new List<int>();

    private static Stack<int> Trilha = new Stack<int>();

    private static Labirinto Labirinto;

    private static bool Running = false;

    private static DateTime TimerIni;

    private static int PosiInicial = 0;
    private static int PosiFinal = 0;
    private static int Distancia = 0;

    private static async Task Main(string[] args) {
        if (args.Length != 2) {
            Console.WriteLine("Passe como argumentos: nome_labirinto, id_labirinto");
            return;
        }

        Labirinto = new Labirinto(args[0], args[1], 0);

        HttpClient client = new HttpClient();

        Thread th = new Thread(ProcessarAsync);

        TimerIni = DateTime.Now;
        th.Start();

        Running = true;

        int y = Console.GetCursorPosition().Top;

        int count = 1;

        while (Running) {
            //Console.SetCursorPosition(0, y);
            //Console.WriteLine($"Executando{new string('.', count),-3}");
            count++;
            if (count == 4) count = 1;
            Thread.Sleep(300);
        }

        var span = DateTime.Now.Subtract(TimerIni);

        var path = PosiFinal;

        List<int> result = new List<int> {
            path
        };

        while (true) {
            if (path == PosiInicial)
                break;
            path = Labirinto.NextPosition(path);
            result.Add(path);
        }

        result.Reverse();

        ValidarCaminho validar = new ValidarCaminho {
            id = Labirinto.LabirintoId,
            labirinto = Labirinto.LabirintoName,
            todos_movimentos = result
        };

        var content = new StringContent(JsonConvert.SerializeObject(validar), Encoding.UTF8, "application/json");

        var resultadoValidacao = JsonConvert.DeserializeObject<ResultadoValidacao>(await (await client.PostAsync("https://gtm.delary.dev/validar_caminho", content)).Content.ReadAsStringAsync());

        Console.WriteLine("\nResultado:");
        Console.WriteLine("Validado: " + resultadoValidacao.caminho_valido);
        Console.WriteLine("Qtd_Movimentos: " + resultadoValidacao.quantidade_movimentos);
        Console.Write("Movimentos: [");
        result.ForEach(e => Console.Write(e + " "));
        Console.WriteLine("]");
        Console.WriteLine($"Tempo: {span.Minutes:00}:{span.Seconds:00}");
    }

    private static async void ProcessarAsync() {
        try {
            MovimentoEnviar movimentoEnviar = new MovimentoEnviar {
                id = Labirinto.LabirintoId,
                labirinto = Labirinto.LabirintoName
            };

            JObject ob = JObject.FromObject(movimentoEnviar);
            ob.Remove("nova_posicao");

            StringContent content = new StringContent(ob.ToString(), Encoding.UTF8, "application/json");

            HttpClient client = new HttpClient();

            Movimento? mov = JsonConvert.DeserializeObject<Movimento>(await (await client.PostAsync("https://gtm.delary.dev/iniciar", content)).Content.ReadAsStringAsync());

            Labirinto.StartPath(mov.Pos_Atual);

            Labirinto.Position = mov.Pos_Atual;
            PosiInicial = mov.Pos_Atual;
            Labirinto.InsertPaths(mov.Movimentos);

            Visitados.Add(mov.Pos_Atual);


            if (mov.Final)
                return;

            while (true) {

                Console.Write($"Pos: {mov.Pos_Atual}, Movs: [");
                mov.Movimentos?.ForEach(e => Console.Write(e + " "));
                Console.WriteLine($"], Final: {mov.Final}");

                int pos = 0;
                var lista = mov.Movimentos ?? new List<int>();

                foreach (var m in lista) {
                    if (Visitados.Contains(m)) 
                            continue;
                    pos = m;
                    break;
                }

                if (pos == 0 || (Distancia <= Labirinto.Cont && Distancia > 0)) {
                    if (Trilha.Count == 0)
                        break;
                    Trilha.TryPop(out pos);
                    Labirinto.Back();
                } else {
                    Labirinto.Foward();
                    Trilha.Push(mov.Pos_Atual);
                }

                movimentoEnviar.nova_posicao = pos;

                content = new StringContent(JsonConvert.SerializeObject(movimentoEnviar), Encoding.UTF8, "application/json");

                string retorno = await (await client.PostAsync("https://gtm.delary.dev/movimentar", content)).Content.ReadAsStringAsync();

                mov = JsonConvert.DeserializeObject<Movimento?>(retorno);

                if (mov == null) {
                    Console.WriteLine("Falha ao ler caminho.");
                    return;
                }

                if (!Visitados.Contains(mov.Pos_Atual))
                    Visitados.Add(mov.Pos_Atual);

                Labirinto.Position = mov.Pos_Atual;
                if (Labirinto.InsertPaths(mov.Movimentos) && PosiFinal != 0)
                    Distancia = Labirinto.ValuePath(PosiFinal);

                if (mov.Final) {
                    PosiFinal = mov.Pos_Atual;
                    Distancia = Labirinto.Cont;
                }
            }
        }
        catch { }
        Running = false;
    }
}