using System.Collections.Generic;

[System.Serializable]
public class WorkerSaveData {
    public float posX;
    public float posY;

    public string jobType; // Enum en string pour sérialisation
    public string campSide; // idem

    public int health;

    public Dictionary<string, int> currencies; // key = CurrencyType.ToString()
}
