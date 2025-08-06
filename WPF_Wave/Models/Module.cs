using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Wave.Models;

public class Module
{
    public string Name { get; set; }

    public Dictionary<string, Variable> ID_Variable_Pairs { get; set; } = [];

    public List<Module> SubModules { get; set; } = [];

    public Module(string name)
    {
        Name = name;
    }
}
