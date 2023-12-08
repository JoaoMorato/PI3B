using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PI3;
public class Movimento {
    public int Pos_Atual { get; set; }
    public bool Inicio { get; set; }
    public bool Final { get; set; }
    public List<int> Movimentos { get; set; }
}
