namespace MultiPartyGame
{
    public class DataStructures
    {
        public struct TriviaQuiz 
        {
            public string Author {get; set;}
            public string Question {get; set;}
            public string[] Answers {get; set;}
            public int CorrectAnswerIndex {get; set;}
        }
    }
}