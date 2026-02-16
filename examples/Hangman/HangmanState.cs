namespace Hangman;

public class HangmanState
{
    private string Word { get; set; } = "PROGRAM";
    private HashSet<char> GuessedLetters { get; } = [ ];
    public int WrongGuesses { get; private set; }
    private const int _maxWrongGuesses = 6;

    public static readonly char[] UsAlphabet =
    [
        'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U',
        'V', 'W', 'X', 'Y', 'Z'
    ];

    public bool IsGameOver => WrongGuesses >= _maxWrongGuesses || IsWordGuessed();
    public bool IsWon => IsWordGuessed() && !IsGameLost;
    public bool IsGameLost => WrongGuesses >= _maxWrongGuesses;

    public bool IsWordGuessed()
    {
        return Word.All( c => GuessedLetters.Contains( c ) );
    }

    public string GetDisplayWord()
    {
        return string.Join( " ", Word.Select( c => GuessedLetters.Contains( c ) ? c : '_' ) );
    }

    public LetterState GetLetterState( char letter )
    {
        if ( !GuessedLetters.Contains( letter ) )
            return LetterState.Unguessed;

        return Word.Contains( letter ) ? LetterState.Correct : LetterState.Wrong;
    }

    public void GuessLetter( char letter )
    {
        if ( IsGameOver || !GuessedLetters.Add( letter ) )
            return;

        if ( !Word.Contains( letter ) )
        {
            WrongGuesses++;
        }
    }

    public void Reset()
    {
        GuessedLetters.Clear();
        WrongGuesses = 0;
        Word = GetRandomWord();
    }

    private static string GetRandomWord()
    {
        string[] words =
        [
            "PROGRAM", "COMPUTER", "KEYBOARD", "MONITOR", "ALGORITHM", "FUNCTION", "VARIABLE", "ARRAY", "LOOP",
            "CONDITION"
        ];
        return words[ Random.Shared.Next( words.Length ) ];
    }
}

public enum LetterState
{
    Unguessed,
    Correct,
    Wrong
}