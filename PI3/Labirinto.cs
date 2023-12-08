using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PI3;
public class Labirinto {
    public string LabirintoId { get; init; }
    public string LabirintoName { get; private set; }
    public int Position { get => _Position; set { _Position = value; } }
    public int Cont { get; private set; }

    private int _Position;
    private Dictionary<int, List<int>> Path;
    private Dictionary<int, int> BackPath;

    public Labirinto(string name, string id, int origem) {
        LabirintoId = id;
        Position = origem;
        LabirintoName = name;
        Cont = 0;
    }

    public void Foward() => Cont++;
    public void Back() => Cont--;

    public int[] Paths(int position) => Path.ContainsKey(position) ? Path[position].ToArray() : Array.Empty<int>();

    public void StartPath(int posi) {
        Path = new Dictionary<int, List<int>>();
        BackPath = new Dictionary<int, int> {

        };
        Cont = 0;
    }

    public bool InsertPaths(List<int> paths) {
        paths ??= new List<int>();
        
        if (!Path.ContainsKey(_Position))
            Path.Add(_Position, paths);

        int menor = Cont;

        paths.ForEach(e =>
        {
            if (!BackPath.TryGetValue(e, out var m))
                return;
            menor = int.Min(m, menor);
        });
        
        Cont = int.Min(menor + 1, Cont);

        if (!BackPath.ContainsKey(_Position))
            BackPath.TryAdd(_Position, Cont);
        else
            BackPath[_Position] = int.Min(BackPath[_Position], Cont);

        bool recont = false;

        foreach (var p in paths) {
            if (!BackPath.ContainsKey(p)) continue;
            if (BackPath[p] <= Cont + 1) continue;
            recont = true;
            Recontar(p, Cont + 1);
        }

        return recont;
    }

    private void Recontar(int posi, int cont) {
        if (!BackPath.TryGetValue(posi, out var aux))
            return;

        if (aux <= cont)
            return;

        BackPath[posi] = cont;

        if (!Path.TryGetValue(posi, out var lista))
            return;

        lista.ForEach(e => Recontar(e, cont + 1));
    }

    public int NextPosition(int position) {
        if (!Path.TryGetValue(position, out var path))
            return 0;

        int result = int.MaxValue;
        int posi = 0;
        int valor = BackPath[position];
        foreach (var p in path) {
            if (!BackPath.TryGetValue(p, out int v))
                continue;

            if (v >= valor)
                continue;

            if (v < result) {
                result = v;
                posi = p;
            }
        }

        return posi;
    }

    public int ValuePath(int position) => BackPath[position];
}
