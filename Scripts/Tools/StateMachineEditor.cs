using Godot;
using System;

namespace OpenARPG.Tools
{
    public partial class StateMachineEditor : GraphEdit
    {
        public void _on_connection_request(string from_node, int from_port, string to_node, int to_port)
        {
            Logger.Log($"CONNECTION REQUEST: \"{from_node}\" {from_port} <-> \"{to_node}\" {to_port}");

            Error error = ConnectNode(from_node, from_port, to_node, to_port);

            foreach(Godot.Collections.Dictionary connection in GetConnectionList())
            {
                Logger.Log($"CONNECTION {connection.ToString()}");
            }
        }

        public void _on_disconnection_request(string from_node, int from_port, string to_node, int to_port)
        {
            Logger.Log($"DISCONNECTION REQUEST: \"{from_node}\" {from_port} <-> \"{to_node}\" {to_port}");

            //Error error = ConnectNode(from_node, from_port, to_node, to_port);
        }
    }
}