# RefreshSwitch

[English](README.md) | Italiano

App per la tray di Windows per cambiare la frequenza di aggiornamento (Hz) di tutti i monitor e per spegnere i monitor senza scollegarli.

- **Clic sinistro**: passa alla frequenza successiva su tutti i monitor
- **Clic destro**: scegli una frequenza per tutti i monitor o per un singolo monitor, spegni gli schermi, disattiva/riattiva un monitor, avvio con Windows, Esci
- L'icona mostra gli Hz del monitor principale; il tooltip elenca tutti i monitor
- I monitor sono mostrati con il nome reale del modello e il numero dello schermo
- Non richiede privilegi di amministratore
- La lingua dell'interfaccia segue Windows (italiano o inglese)

Se un monitor non supporta la frequenza richiesta, usa la più vicina tra quelle disponibili alla sua risoluzione attuale.

## Spegnere i monitor

- **Spegni schermi**: mette tutti i monitor in standby; si riaccendono muovendo il mouse o premendo un tasto.
- **Disattiva [monitor]**: stacca il monitor dal desktop, come scollegare il cavo (le finestre si spostano sugli altri schermi). Con **Riattiva [monitor]** lo riporti com'era: posizione, risoluzione e Hz vengono salvati e ripristinati. L'ultimo monitor attivo non può essere disattivato.

Gli errori vengono salvati in `%LOCALAPPDATA%\RefreshSwitch\error.log`.

## Compilazione

```
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe -nologo -target:winexe -out:RefreshSwitch.exe -r:System.Windows.Forms.dll -r:System.Drawing.dll -r:System.Management.dll RefreshSwitch.cs
```
