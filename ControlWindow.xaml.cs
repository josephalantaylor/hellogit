using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Windows.Threading;
using Microsoft.Win32;

using MathingBee.Domain;
using MathingBee.SoundControl;

namespace MathingBee.Presentation
{
    /// <summary>
    /// Interaction logic for Presentation.ControlWindow.xaml
    /// </summary>
    public partial class ControlWindow : Window
    {
        #region Members
        BeeCompetition Bee;
        DisplayWindow Display;
        SoundControler Sound;
        DispatcherTimer QuestionTimer;
        #endregion

        #region initialization
        public ControlWindow()
        {
            InitializeComponent();
            Bee = new BeeCompetition();
            Sound = new SoundControler();
            QuestionTimer = new DispatcherTimer();
            QuestionTimer.Interval = new TimeSpan(0, 0, 1);
            QuestionTimer.Tick += new EventHandler(OnTimerTick);
            SetupRow.Height = new GridLength();
            ControlRow.Height = new GridLength(0);
            SetStatus();
        }
        #endregion

        #region public functions
        public void SetStatus()
        {
            bool Ready = Bee.Valid;
            StatusLabel.Text = Bee.StatusDetails;
            if ((Ready) && (Display == null))
                SetupDoneButton.Content = "Activate display window";
            else
                SetupDoneButton.Content = Bee.Status;
            SetupDoneButton.IsEnabled = Ready;
        }
        public void ShowRounds()
        {
            RoundList.Items.Clear();
            RoundDisplay l;
            foreach (BeeRound round in Bee.Rounds)
            {
                l = new RoundDisplay() { DataContext = round };
                RoundList.Items.Add(l);
            }
        }
        public void ShowContestants()
        {
            ContestantList.ItemsSource = Bee.RoundContestants;
        }
        #endregion

        #region animations
        void AnimateStart()
        {
            Sound.Play(SoundType.Startup);
            Display.AnimateStart();
        }
        void AnimateNewRound()
        {
            Sound.Play(SoundType.NewRound, Bee.RoundNumber);
            Display.AnimateNewRound(Bee.RoundNumber);
        }
        void AnimateEndOfRound()
        {
            Sound.Play(SoundType.EndRound);
            MessageBox.Show("Round ended", "Round ended");
        }
        void AnimateNextContestant()
        {
            Sound.Play(SoundType.NextContestant);
        }
        void AnimateCorrect()
        {
            Sound.Play(SoundType.Correct);
        }
        void AnimateIncorrect(bool TimeUp)
        {
            if (TimeUp)
                Sound.Play(SoundType.TimesUp);
            else
                Sound.Play(SoundType.Incorrect);
        }
        void AnimateSecondChanceGrade()
        {
            Sound.Play(SoundType.SecondChanceGrade);
        }
        void AnimateSecondChanceAll()
        {
            Sound.Play(SoundType.SecondChanceAll);
        }
        void AnimateGradeWinner(string Winners)
        {
            Sound.Play(SoundType.Winner);
            Display.AnimateGradeWinners(Winners);
        }
        void AnimateOverallWinner(string Winner, string Winners)
        {
            Sound.Play(SoundType.GrandPrize);
            Display.AnimateAllWinners(Winner, Winners);
        }
        #endregion

        #region interaction functions
        void DisplayUpdate()
        {
            // buttons enabled & captioned
            bool Questioning = Bee.SubState == BeeSubState.Questioning;
            JudgeButton.IsEnabled = !Questioning;
            CorrectButton.IsEnabled = IncorrectButton.IsEnabled = Questioning;
            switch (Bee.State)
            {
                case BeeState.Ready:
                    NextQuestionButton.Content = "Begin the Mathing Bee!";
                    NextQuestionButton.IsEnabled = true;
                    break;
                case BeeState.Running:
                    switch (Bee.SubState)
                    {
                        case BeeSubState.Questioning:
                            NextQuestionButton.Content = "Show Next Question";
                            NextQuestionButton.IsEnabled = false;
                            break;
                        case BeeSubState.RoundBeginning:
                        case BeeSubState.Waiting:
                            NextQuestionButton.Content = "Show Next Question";
                            NextQuestionButton.IsEnabled = true;
                            break;
                        case BeeSubState.RoundEnding:
                            NextQuestionButton.Content = "Begin Next Round";
                            NextQuestionButton.IsEnabled = true;
                            break;
                        default:
                            NextQuestionButton.Content = "Not Ready";
                            NextQuestionButton.IsEnabled = false;
                            break;
                    }
                    break;
                case BeeState.Ended:
                    NextQuestionButton.Content = "Mathing Bee Ended";
                    NextQuestionButton.IsEnabled = false;
                    break;
                default:
                    NextQuestionButton.Content = "Not Ready";
                    NextQuestionButton.IsEnabled = false;
                    break;
            }

            // contestants
            string Contestants = String.Format(
                "{0}\n{1}\n{2}",
                Bee.CurrentContestant == null ? "" : Bee.CurrentContestant.ToString(),
                Bee.NextContestant == null ? "" : Bee.NextContestant.ToString(),
                Bee.FollowingContestant == null ? "" : Bee.FollowingContestant.ToString()
                ).Trim();
            string Contestant = String.Format("{0}", Bee.CurrentContestant == null ? "" : Bee.CurrentContestant.ToString());
            if ((Contestant != "") && (Contestants.IndexOf(Contestant) == 0))
                if (Contestant == Contestants)
                    Contestants = "";
                else
                    Contestants = Contestants.Substring(Contestant.Length + 1);

            // show questions and answers
            bool QuestionShow = (Bee.State == BeeState.Running) && (Bee.SubState != BeeSubState.RoundBeginning);
            bool AnswerShow = (Bee.SubState == BeeSubState.Waiting) || (Bee.SubState == BeeSubState.RoundEnding);

            // control window
            NextContestant.Content = Contestants;
            RoundDisp.Content = String.Format("{0}", Bee.RoundNumber);
            QuestionsDisp.Content = Bee.RoundNumber > 0 ? Bee.CurrentRound.UnusedCount.ToString() : "0";
            ContestantsDisp.Content = String.Format("{0}", Bee.RemainingCount);
            RoundContestantsDisp.Content = Bee.ContestainsRemainingInRound;
            ContestantDisp.Content = Contestant;

            // display window
            Display.RoundDisplay.Content = String.Format("Round {0}", Bee.RoundNumber);
            Display.RoundConcept.Text = Bee.RoundNumber > 0 ? String.Format("{0}\n{1} seconds", Bee.CurrentRound.Concept, Bee.CurrentRound.Seconds.Seconds) : "";
            Display.QuestionConceptDisplay.Content = QuestionShow ? Bee.CurrentQuestion.Concept : "";
            Display.ContestantDisplay.Content = Contestant;
            Display.ContestantList.Text = Contestants;

            // question and answer
            if (QuestionShow)
            {
                Brush c = (Bee.SubState == BeeSubState.Waiting || Bee.SubState == BeeSubState.RoundEnding) ? Bee.LastCorrect ? Brushes.Lime : Brushes.Red : Brushes.White;
                ContestantDisp.Foreground = c;
                MathRender.Render(Bee.CurrentQuestion.Question, QuestionPreview, 16, c);
                MathRender.Render(Bee.CurrentQuestion.Question, Display.QuestionDisplay, AnswerShow ? 10 : 18, c);
                MathRender.Render(Bee.CurrentQuestion.Answer, AnswerPreview, 16, c);
                if (AnswerShow)
                    MathRender.Render(Bee.CurrentQuestion.Answer, Display.AnswerDisplay, 36, c);
                else
                    MathRender.Render("", Display.AnswerDisplay, 18, c);
            }
            else
            {
                MathRender.Render(" \t ", QuestionPreview, 16, Brushes.White);
                MathRender.Render(" \t ", Display.QuestionDisplay, 18, Brushes.White);
                MathRender.Render(" \t ", AnswerPreview, 16, Brushes.White);
                MathRender.Render("", Display.AnswerDisplay, 18, Brushes.White);
            }

            // extras
            RoundList.Items.Refresh();
            QuestionList.Items.Refresh();
            ContestantList.Items.Refresh();
        }
        void ReadyButtonsForNextRound()
        {
            Thread.Sleep(2000);
            AnimateEndOfRound();
            bool grade;
            if (Bee.SecondChanceNeeded(out grade))
            {
                Thread.Sleep(2000);
                if (grade)
                    AnimateSecondChanceGrade();
                else
                    AnimateSecondChanceAll();
            }
            else if (Bee.OverallWon())
            {
                Thread.Sleep(2000);
                string s = "The overall winner is " + Bee.OverallWinner.ToString() + "\n\nThe grade winners are:\n";
                string s2 = "";
                foreach (BeeContestant c in Bee.GradeWinners)
                    s2 += c.ToString() + "\n";
                s2 = s2.Trim();
                s += s2;
                AnimateOverallWinner(Bee.OverallWinner.ToString(), s2);
                MessageBox.Show(s.Trim(), "Overall Winner");
                Display.StopAnimateWinners();
            }
            else if (Bee.GradeWon())
            {
                Thread.Sleep(2000);
                string s = "";
                foreach (BeeContestant c in Bee.GradeWinners)
                    s += c.ToString() + "\n";
                AnimateGradeWinner(s);
                MessageBox.Show("These are our grade winners so far:\n" + s.Trim(), "Grade Winners");
                Display.StopAnimateWinners();
            }
            DisplayUpdate();
        }
        void SetupDisplay()
        {
            Display = new DisplayWindow();
            Display.Show();
            SetStatus();
        }
        void SetupBee()
        {
            Bee.Start();
            SetupRow.Height = new GridLength(0);
            ControlRow.Height = new GridLength();
            ShowRounds();
            ShowContestants();
            DisplayUpdate();

            Height = 470;
            //Point p = JudgeButton.TranslatePoint(new Point(0, 0), this);
            //this.Height = p.Y + JudgeButton.ActualHeight + JudgeButton.ActualHeight + 4;
            
            AnimateStart();
        }
        void TimerUp()
        {
            Bee.TimeRemaining--;
            TimerDisp.Content = String.Format("Timer: {0}", Bee.TimeRemaining);
            Display.SetTime(Bee.TimeRemaining);
            if (Bee.TimeRemaining == 0)
                Incorrect(true);
        }
        void Next()
        {
            if (Bee.State == BeeState.Ready)
            {
                // start first round
                Bee.NextRound();
                DisplayUpdate();
                AnimateNewRound();
            }
            else
            {
                if (Bee.SubState == BeeSubState.RoundEnding)
                {
                    // start next round
                    Bee.NextQuestion(); // this will advance the competition to the next round
                    DisplayUpdate();
                    AnimateNewRound();
                }
                else
                {
                    // show next question
                    Bee.NextQuestion();
                    DisplayUpdate();
                    Bee.SetTimeRemaining();
                    TimerDisp.Content = String.Format("Timer: {0}", Bee.TimeRemaining);
                    Display.SetTime(Bee.TimeRemaining);
                    QuestionTimer.Start();
                    AnimateNextContestant();
                }
            }
        }
        System.Action WaitForRender()
        {
            return null;
        }
        void Correct()
        {
            QuestionTimer.Stop();
            bool NewRound = Bee.Correct();
            DisplayUpdate();
            Dispatcher.Invoke(WaitForRender, DispatcherPriority.Render);
            Display.UpdateLayout();
            AnimateCorrect();
            if (NewRound)
                ReadyButtonsForNextRound();
        }
        void Incorrect(bool TimeUp = false)
        {
            QuestionTimer.Stop();
            bool NewRound = Bee.Incorrect();
            DisplayUpdate();
            Dispatcher.Invoke(WaitForRender, DispatcherPriority.Render);
            ContestantsDisp.Content = String.Format("{0}", Bee.RemainingCount);
            AnimateIncorrect(TimeUp);
            if (NewRound)
                ReadyButtonsForNextRound();
        }
        void Judge()
        {
            JudgeWindow jw = new JudgeWindow();
            jw.Bee = Bee;
            jw.ShowDialog();
            DisplayUpdate();
        }
        #endregion

        #region event handlers
        private void CreateMathButton_Click(object sender, RoutedEventArgs e)
        {
            CreateMathWindow w = new CreateMathWindow() { Bee = Bee };
            w.ShowDialog();
            SetStatus();
        }

        private void ChoseMathFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Mathing Bee Math Files|*.BeeM|All Files|*.*";
            if (dlg.ShowDialog() == true)
            {
                Bee.LoadMath(dlg.FileName);
                if (!Bee.ValidMath)
                    MessageBox.Show("The Mathing Bee file you selected couldn't be loaded.", "Error");
            }
            SetStatus();
        }

        private void ChooseContestantsButton_Click(object sender, RoutedEventArgs e)
        {
            ChooseContestantsWindow w = new ChooseContestantsWindow();
            w.SetBee(Bee);
            w.ShowDialog();
            SetStatus();
        }

        private void SetupDoneButton_Click(object sender, RoutedEventArgs e)
        {
            if (Display == null)
                SetupDisplay();
            else
                SetupBee();
        }

        private void RoundList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            QuestionList.Items.Clear();
            QuestionDisplay l;
            foreach(BeeQuestion question in Bee.Rounds[RoundList.SelectedIndex].Questions)
            {
                l = new QuestionDisplay() { DataContext = question };
                QuestionList.Items.Add(l);
            }
        }

        private void NextQuestionButton_Click(object sender, RoutedEventArgs e)
        {
            Next();
        }

        private void CorrectButton_Click(object sender, RoutedEventArgs e)
        {
            Correct();
        }

        private void IncorrectButton_Click(object sender, RoutedEventArgs e)
        {
            Incorrect();
        }

        private void JudgeButton_Click(object sender, RoutedEventArgs e)
        {
            Judge();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Display != null)
            {
                Display.CanClose = true;
                Display.Close();
            }
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            TimerUp();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            Point p = SetupDoneButton.TranslatePoint(new Point(0, 0), this);
            this.Height = p.Y + SetupDoneButton.ActualHeight + SetupDoneButton.ActualHeight + 4;
        }
        #endregion
    }
}
