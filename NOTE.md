# Note Progettuali

Di seguito sono riportate le assunzioni dedotte dal [documento di progetto](./TRACCIA.md) e le scelte progettuali effettuate.

## Analisi del Servizio

L'applicazione agisce come controller per un amplificatore remoto. La comunicazione avviene su socket TCP (porta 23) con un protocollo custom basato su stringhe terminate da carriage return (`\r`).

### Requisiti Core

- **Persistenza:** Connessione sempre attiva con gestione automatica del fault.
- **Inizializzazione:** Handshake obbligatorio (`INIT`) solo al primo aggancio.
- **Parsing Dinamico:** Elaborazione asincrona dei dati di stato (Power ON/OFF) e telemetria (Volume).

## Scelte Progettuali

Scelte progettuali effettuate:

- [Stack](#stack)
- [Architettura](#architettura)
- [Gestione della Rete](#gestione-della-rete-socket--resilience)
- [Buffering e Integrità Dati](#buffering-e-integrità-dati)
- [Logica di Elaborazione](#logica-di-elaborazione-parsing)
- [Sicurezza e Robustezza](#sicurezza-e-robustezza)

### Stack

- **Linguaggio:** C# su .NET 9 (LTS), versione stabile e con supporto lungo.
- **Tipo progetto:** Console Application — sufficiente per lo scopo, senza overhead di framework UI o web.
- **Librerie:** solo BCL (Base Class Library) standard, senza dipendenze esterne. La gestione del socket TCP è affidata a `System.Net.Sockets.TcpClient`, che offre un'API sufficientemente astratta senza sacrificare il controllo sul flusso di dati.

### Architettura

Ho suddiviso la logica del programma per seguire i principi HEXAGONAL e SOLID. In particolare:

- `src/Models/AmplifierState.cs` è la **SoT** dell'applicazione.
- `src/Core/IAudioClient.cs` è l'interfaccia (il _Port_) che definisce le azioni che può fare un client. Può essere estesa da più _Adapter_ in caso variazioni tecnologiche.
- `src/Infrastructure/TcpAudioClient.cs` è l'adapter con l'implementazione effettiva.
- `src/Infrastructure/ResponseParser.cs` gestisce la logica di "traduzione".
- `src/Infrastructure/Logger.cs` per formattare e colorare i messaggi di log in console.
- `src/Services/AmplifierManager.cs` è l'analogo dell'use-case per il DDD e mette insieme il client (adapter) per orchestrare la connessione, il parser per gestire il messaggio e si occupa di aggiornare lo stato (modello).
- `src/Program.cs` è l'entrypoint che crea le istanze (_Dependency Injection_ manuale).

### Gestione della Rete (Socket & Resilience)

Invece di un approccio sincrono, ho strutturato il client per operare in modo non bloccante.

- **Auto-Reconnect:** Il `ConnectionLoop` in `AmplifierManager` monitora continuamente lo stato del socket. Quando il server cade o la connessione si interrompe, il loop gestisce automaticamente il ritentativo dopo un'attesa di 4 secondi, evitando di saturare la rete con tentativi ravvicinati.
- **Protocollo di Inizializzazione:** Un flag booleano (`_isFirstConnection`) garantisce che il comando `INIT\r` venga inviato esclusivamente alla prima connessione riuscita dell'intera sessione, indipendentemente da quante volte il client si riconnette.

### Buffering e Integrità Dati

Il TCP non garantisce che un messaggio arrivi intero in un singolo pacchetto — può arrivare spezzato in più read, oppure più messaggi possono arrivare concatenati nello stesso buffer.

- **Stream Buffering:** Ho implementato uno `StringBuilder` che accumula i byte ricevuti carattere per carattere, emettendo un messaggio completo solo quando viene rilevato il terminatore `\r`. Questo garantisce che il parser riceva sempre stringhe intere, indipendentemente dalla frammentazione del livello TCP.
- **Validazione:** Ogni stringa completata viene validata prima di essere passata al parser, scartando silenziosamente le stringhe vuote per evitare eccezioni su dati malformati.

### Logica di Elaborazione (Parsing)

Il parsing del volume (`MVxxx`) richiede una trasformazione da stringa a valore numerico con arrotondamento.

- **Conversione Decimale:** Il suffisso numerico (es. `705`) viene diviso per 10 per ottenere il valore decimale reale (`70.5`), poi arrotondato verso il basso tramite `Math.Floor` come richiesto dalle specifiche. La conversione usa `CultureInfo.InvariantCulture` per essere robusta rispetto alle impostazioni locali del sistema operativo.
- **Mapping Stati:** Le stringhe di stato (`ZMON`/`ZMOFF`) sono gestite con un semplice controllo sul suffisso, evitando lookup su dizionari non necessari per un protocollo così semplice.
- **Comandi Sconosciuti:** Il parser lancia un'eccezione per comandi non riconosciuti, che viene catturata e loggata come `Warning` dall'`AmplifierManager`. Questo permette al sistema di continuare a funzionare anche se il server invia stringhe non previste dal protocollo documentato.

### Sicurezza e Robustezza

Dato il contesto applicativo, ho prestato attenzione a:

- **Resource Management:** I socket e gli stream vengono sempre disposti esplicitamente, sia in caso di disconnessione ordinata che di errore. I riferimenti vengono azzerati dopo il `Dispose` per prevenire double-free e accessi a oggetti già rilasciati.
- **Thread Safety:** Il flag `_isConnected` è dichiarato `volatile` per garantire la visibilità corretta tra il thread di ricezione e il thread principale del `ConnectionLoop`, senza dover ricorrere a lock più pesanti.
