# Esercizio di Programmazione C#

L'obbiettivo del seguente esercizio è quello di creare un programma console in C# al cui interno è definito un **TCP/IP Client** che si connetta ad un **TCP/IP server** (Indirizzo IP "192.168.70.30" alla porta "23"). Il Server a cui l'applicativo si connette rappresenta un amplificatore audio.

## Requisiti Tecnici del Client

- **Gestione Connessione:** Il Client deve essere in grado di gestire la riconnessione automatica al server (se la connessione cade) e tutti i vari errori.
- **Monitoraggio:** Tutti i cambi di stato della connessione al server devono essere stampati in console (connesso, disconnesso, errore etc..).
- **Inizializzazione:** Alla prima (e solamente alla prima) connessione al server, il client deve inviare la seguente stringa: `"INIT\r"`.
- **Persistenza:** Mantenere sempre la connessione aperta finché l'applicativo è in esecuzione e gestire le riconnessioni.
- **Ascolto Attivo:** Rimanere sempre in ascolto delle stringhe ed elaborarle ogni qualvolta vengano ricevute, poiché l'amplificatore potrebbe inviare dati anche non richiesti.
- **Buffering:** Gestire la possibilità che le stringhe vengano inviate in più pacchetti facendo un buffering della ricezione fino al completamento della stringa ricevuta.

## Protocollo di Comunicazione

Il Server risponderà inviando una serie di stringhe che il client dovrà elaborare:

1.  **Stato del Sistema:**

- `"ZMON\r"` (SISTEMA ON): Stampare in console che il sistema è acceso.
- `"ZMOFF\r"` (SISTEMA OFF): Stampare in console che il sistema è spento.

2.  **Livello del Volume:**

- La stringa inizia con `MV` seguita da 3 numeri che rappresentano il valore da 0 a 100.
- Esempi di conversione:
  - `"MV705\r"` -> 70.5 -> Arrotondare a numero intero **70**.
  - `"MV502\r"` -> 50.2 -> Arrotondare a numero intero **50**.
  - `"MV600\r"` -> 60.0 -> Arrotondare a numero intero **60**.
- Il valore del volume, arrotondato e trasformato in numero intero, deve essere stampato in console ogni volta che viene ricevuto.

## Note per lo Sviluppo e Consegna

- **Test:** È possibile usare un software su Windows per simulare il server e testare il codice (modificando l'IP). Il codice finale verrà testato con un server reale presso gli uffici dell'azienda.
- **Repository:** Creare un repository **GIT (Privato)** dove ospitare il codice per la visione senza bisogno di scambiarsi file.
