# RefreshSwitch

[English](README.md) | Italiano

App per la tray di Windows per cambiare la frequenza di aggiornamento (Hz) di tutti i monitor e per spegnere i monitor senza scollegarli.

- **Cambia frequenza con un clic sull'icona** (disattivata di default): se la abiliti dal menu, il clic sinistro passa alla frequenza successiva su tutti i monitor; il menu resta disponibile con il clic destro
- **Clic** sull'icona (sinistro o destro) per aprire il menu: scegli una frequenza per tutti i monitor o per un singolo monitor, spegni gli schermi, disattiva/riattiva un monitor, avvio con Windows, Esci
- L'icona mostra gli Hz del monitor principale; il tooltip elenca tutti i monitor
- I monitor sono mostrati con il nome reale del modello e il numero dello schermo
- Non richiede privilegi di amministratore
- La lingua dell'interfaccia segue Windows (italiano o inglese)

Se un monitor non supporta la frequenza richiesta, usa la più vicina tra quelle disponibili alla sua risoluzione attuale.

## A chi serve

Consiglio RefreshSwitch a chi ha bisogno di scollegare un monitor come se staccasse fisicamente il cavo, ma senza toccarlo.

Un esempio è guardare Netflix in 4K da Microsoft Edge (serve un'estensione del browser): un secondo schermo collegato al PC può essere d'intralcio, e di solito la soluzione è staccarlo. Con **Disattiva [monitor]** lo schermo esce dal desktop allo stesso modo, e con **Riattiva tutti i monitor** torna al suo posto quando hai finito.

È comodo anche se cambi spesso frequenza di aggiornamento, per esempio alta per i giochi e più bassa per consumare meno o per adattarla a un video.

## Spegnere i monitor

- **Spegni schermi**: mette tutti i monitor in standby; si riaccendono muovendo il mouse o premendo un tasto.
- **Disattiva [monitor]**: stacca il monitor dal desktop, come scollegare il cavo (le finestre si spostano sugli altri schermi). L'ultimo monitor attivo non può essere disattivato.
- **Riattiva tutti i monitor (Estendi)**: fa la stessa cosa di "Estendi questi schermi" nelle impostazioni di Windows, quindi ogni monitor collegato torna con la disposizione che Windows ricorda.

## Stili dell'icona

Dieci stili, selezionabili dal menu, mostrati qui su barra scura e chiara. Quelli monocromatici seguono il tema di Windows. I numeri gotici sono disegnati dall'app stessa (un pennino da calligrafia fatto scorrere lungo ogni cifra).

![Stili icona](icon-styles.png)

Gli errori vengono salvati in `%LOCALAPPDATA%\RefreshSwitch\error.log`.

## Come ottenere l'app (senza saper programmare)

Non devi installare nulla: l'app si crea con uno strumento già presente in Windows 10 e 11.

1. In cima a questa pagina clicca il pulsante verde **Code**, poi **Download ZIP**.
2. Apri la cartella Download, fai clic destro sul file ZIP e scegli **Estrai tutto...**, poi **Estrai**.
3. Apri la cartella estratta e fai doppio clic su **`build.bat`** (può comparire solo come `build`).
   - Se appare una finestra blu "PC protetto da Windows", clicca **Ulteriori informazioni** e poi **Esegui comunque**. Compare perché il file è stato scaricato da internet.
4. Si apre una finestra nera che dopo un attimo scrive **Fatto**. Premi un tasto per chiuderla.
5. Nella stessa cartella ora c'è **`RefreshSwitch.exe`**: fai doppio clic per avviare l'app. Non serve il permesso di amministratore.
6. L'icona compare nella tray, accanto all'orologio. Se non la vedi, clicca la freccetta **^**; puoi trascinare l'icona sulla barra per averla sempre in vista.

Per avviarla in automatico, clicca l'icona e spunta **Avvia con Windows**. Prima puoi spostare la cartella dove preferisci: se la sposti dopo, togli e rimetti la spunta.

Per aggiornare, scarica di nuovo lo ZIP e ripeti i passaggi. Prima chiudi l'app (clic sull'icona, poi **Esci**), altrimenti il file non può essere sostituito.

### Da riga di comando

Se preferisci, questo è il comando che `build.bat` esegue:

```
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe -nologo -target:winexe -out:RefreshSwitch.exe -r:System.Windows.Forms.dll -r:System.Drawing.dll -r:System.Management.dll RefreshSwitch.cs Gothic.cs
```
