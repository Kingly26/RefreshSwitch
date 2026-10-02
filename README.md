# RefreshSwitch

App per la tray di Windows per cambiare la frequenza di aggiornamento (Hz) di tutti i monitor.

- **Clic sinistro**: passa alla frequenza successiva su tutti i monitor
- **Clic destro**: scegli una frequenza per tutti i monitor o per un singolo monitor, avvio con Windows, Esci
- L'icona mostra gli Hz del monitor principale; il tooltip elenca tutti i monitor
- Non richiede privilegi di amministratore

Se un monitor non supporta la frequenza richiesta, usa la più vicina tra quelle disponibili alla sua risoluzione attuale.

## Compilazione

```
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe -nologo -target:winexe -out:RefreshSwitch.exe -r:System.Windows.Forms.dll -r:System.Drawing.dll -r:System.Management.dll RefreshSwitch.cs
```
