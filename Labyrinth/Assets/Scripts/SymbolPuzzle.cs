using System.Collections.Generic;
using UnityEngine;

// Prüft die Reihenfolge der Platten. Erstellt sich selbst (eines pro Szene).
public class SymbolPuzzle : MonoBehaviour
{
    // Lösung (Platzhalter, passt zu den Hinweisen): Mond -> Stern -> Sonne -> Auge
    private static readonly Symbol[] solution = { Symbol.Mond, Symbol.Stern, Symbol.Sonne, Symbol.Auge };
    private const float HungerPenalty = 15f;

    private static SymbolPuzzle instance;
    public static SymbolPuzzle Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameObject("SymbolPuzzle").AddComponent<SymbolPuzzle>();
            }
            return instance;
        }
    }

    private readonly List<SymbolPlate> plates = new List<SymbolPlate>();
    private KeyAltar altar;
    private int progress;

    public void Register(SymbolPlate plate)
    {
        plates.Add(plate);
        // Nach einem Tod: gelöstes Rätsel bleibt gelöst
        if (GameManager.Instance.PuzzleSolved) plate.SetActive(true);
    }

    public void Register(KeyAltar keyAltar)
    {
        altar = keyAltar;
    }

    public void Press(SymbolPlate plate)
    {
        GameManager gm = GameManager.Instance;
        if (gm.PuzzleSolved || plate.IsActive) return;

        if (plate.symbol == solution[progress])
        {
            plate.SetActive(true);
            progress++;
            if (progress >= solution.Length) Solve(gm);
        }
        else
        {
            Fail(gm);
        }
    }

    private void Solve(GameManager gm)
    {
        gm.PuzzleSolved = true;
        if (altar != null) altar.SpawnKey();
    }

    private void Fail(GameManager gm)
    {
        gm.LoseHunger(HungerPenalty);
        progress = 0;
        foreach (SymbolPlate p in plates) p.FlashWrong();
    }
}