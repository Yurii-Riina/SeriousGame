using System;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static Action<ClientAI> OnClientAngry;
    public static Action<ClientAI> OnClientServed;
    public static Action<ClientAI> OnClientStartedOrdering;

    public static void ClientGotAngry(ClientAI client) => OnClientAngry?.Invoke(client);
    public static void ClientWasServed(ClientAI client) => OnClientServed?.Invoke(client); 
    public static void ClientHasStartedOrdering(ClientAI client) => OnClientStartedOrdering?.Invoke(client);

}