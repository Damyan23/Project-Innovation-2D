using System.Collections.Generic;
using UnityEngine;

namespace Mirror.Discovery
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Network/Network Discovery HUD")]
    [HelpURL("https://mirror-networking.gitbook.io/docs/components/network-discovery")]
    [RequireComponent(typeof(NetworkDiscovery))]
    public class NetworkDiscoveryHUD : MonoBehaviour
    {
        readonly Dictionary<long, ServerResponse> discoveredServers = new Dictionary<long, ServerResponse>();
        Vector2 scrollViewPos = Vector2.zero;

        public NetworkDiscovery networkDiscovery;

#if UNITY_EDITOR
        void OnValidate()
        {
            if (networkDiscovery == null)
            {
                networkDiscovery = GetComponent<NetworkDiscovery>();
                UnityEditor.Events.UnityEventTools.AddPersistentListener(networkDiscovery.OnServerFound, OnDiscoveredServer);
                UnityEditor.Undo.RecordObjects(new Object[] { this, networkDiscovery }, "Set NetworkDiscovery");
            }
        }
#endif

        void OnGUI()
        {
            if (NetworkManager.singleton == null)
                return;

            if (!NetworkClient.isConnected && !NetworkServer.active && !NetworkClient.active)
                DrawGUI();

            if (NetworkServer.active || NetworkClient.active)
                StopButtons();
        }

        void DrawGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 500));
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Find Servers"))
                StartDiscovery();

            if (GUILayout.Button("Start Host"))
                StartHost();

            if (GUILayout.Button("Start Server"))
                StartServer();

            GUILayout.EndHorizontal();
            GUILayout.Label($"Discovered Servers [{discoveredServers.Count}]:");

            scrollViewPos = GUILayout.BeginScrollView(scrollViewPos);
            foreach (ServerResponse info in discoveredServers.Values)
                if (GUILayout.Button(info.EndPoint.Address.ToString()))
                    Connect(info);
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        void StopButtons()
        {
            GUILayout.BeginArea(new Rect(10, 40, 100, 25));

            if (NetworkServer.active && NetworkClient.isConnected)
            {
                if (GUILayout.Button("Stop Host"))
                    StopHost();
            }
            else if (NetworkClient.isConnected)
            {
                if (GUILayout.Button("Stop Client"))
                    StopClient();
            }
            else if (NetworkServer.active)
            {
                if (GUILayout.Button("Stop Server"))
                    StopServer();
            }

            GUILayout.EndArea();
        }

        public void StartDiscovery()
        {
            discoveredServers.Clear();
            networkDiscovery.StartDiscovery();
        }

        public void StartHost()
        {
            discoveredServers.Clear();
            NetworkManager.singleton.StartHost();
            networkDiscovery.AdvertiseServer();
        }

        public void StartServer()
        {
            discoveredServers.Clear();
            NetworkManager.singleton.StartServer();
            networkDiscovery.AdvertiseServer();
        }

        public void StopHost()
        {
            NetworkManager.singleton.StopHost();
            networkDiscovery.StopDiscovery();
        }

        public void StopClient()
        {
            NetworkManager.singleton.StopClient();
            networkDiscovery.StopDiscovery();
        }

        public void StopServer()
        {
            NetworkManager.singleton.StopServer();
            networkDiscovery.StopDiscovery();
        }

        public void Connect(ServerResponse info)
        {
            networkDiscovery.StopDiscovery();
            NetworkManager.singleton.StartClient(info.uri);
        }

        public void OnDiscoveredServer(ServerResponse info)
        {
            discoveredServers[info.serverId] = info;
        }
    }
}
