using ReGoap.Unity;

public class StorytellingAgent : ReGoapAgent<string, object>
{
    int Score;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        Score = 0;
    }

    public void AddToScore(int value)
    {
        Score += value;
    }

    public int GetScore()
    {
        return Score;
    }
}
