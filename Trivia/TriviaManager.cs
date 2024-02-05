using Godot;
using MultiPartyGame;
using System;
using System.Collections.Generic;

public partial class TriviaManager : Node
{
    public static TriviaManager Instance { get; private set; }

    public List<DataStructures.TriviaQuiz> TriviaQuizzes = new List<DataStructures.TriviaQuiz>();

    public override void _Ready()
    {
        Instance = this;
    }

    public override void _Process(double delta)
    {
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    public void ReceiveTrivia(string author, string question, string[] answers, int correctAnswerIndex)
    {
        DataStructures.TriviaQuiz quiz = new DataStructures.TriviaQuiz
        {
            Author = author,
            Question = question,
            Answers = answers,
            CorrectAnswerIndex = correctAnswerIndex
        };

        if (!TriviaQuizzes.Contains(quiz))
        {
            TriviaQuizzes.Add(quiz);
        }
        if (!Multiplayer.IsServer())
        {
            RpcId(MultiplayerMenu.HOST_ID, "ReceiveTrivia", quiz.Author, quiz.Question, quiz.Answers, quiz.CorrectAnswerIndex);
        }
        if (Multiplayer.IsServer())
        {
            foreach (int peerId in Multiplayer.GetPeers())
            {
                if (peerId != Multiplayer.GetRemoteSenderId())
                {
                    RpcId(peerId, "ReceiveTrivia", quiz.Author, quiz.Question, quiz.Answers, quiz.CorrectAnswerIndex);
                }
            }
        }
    }
}
