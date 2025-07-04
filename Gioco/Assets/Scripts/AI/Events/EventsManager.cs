using System;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static Action<ClientAI> OnClientGotAngry;
    public static Action<ClientAI> OnClientWasServed;
    public static Action<ClientAI> OnClientStartedOrdering;

    public static void ClientGotAngry(ClientAI client) => OnClientGotAngry?.Invoke(client);
    public static void ClientWasServed(ClientAI client) => OnClientWasServed?.Invoke(client); 
    public static void ClientHasStartedOrdering(ClientAI client) => OnClientStartedOrdering?.Invoke(client);

}