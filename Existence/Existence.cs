using ideal.agent;
using ideal.coupling;
using System.Collections.Generic;

namespace ideal
{
    // Lines 119, 160, and 289 need messages to be passed out.
    public class Existence
    {
        public enum Mood { SelfSatisfied, Frustrated, Bored, Pained, Pleased };

        Mood mood;
        int selfSatisfactionCounter = 0;
        Experiment experience;
        Experiment previousExperience;
        Interaction _enactedInteraction;
        bool enhanced;

        //public delegate void MessageDelegate(object sender, MessageDelegateEventArgs e);
        MessageDelegateEventArgs args = new MessageDelegateEventArgs();
        public event System.EventHandler<MessageDelegateEventArgs> MessageToPass;

        Dictionary<string, Experiment> Experiences = new Dictionary<string, Experiment>();
        Dictionary<string, Result> Results = new Dictionary<string, Result>();
        Dictionary<string, Interaction> Interactions = new Dictionary<string, Interaction>();

        int BoredomLevel = 4;

        public string LABEL_E1 = "e1";
        public string LABEL_E2 = "e2";
        public string LABEL_R1 = "r1";
        public string LABEL_R2 = "r2";

        public Existence(int boredomLevel)
        {
            BoredomLevel = boredomLevel;
            InitExistence(false);
        }
        public Existence(bool enhancedExistence)
        {
            enhanced = enhancedExistence;
            InitExistence(enhancedExistence);
        }

        protected void InitExistence(bool enhanced)
        {
            if (enhanced)
            {
                Experiment e1 = AddOrGetExperience(LABEL_E1);
                Experiment e2 = AddOrGetExperience(LABEL_E2);
                Result r1 = CreateOrGetResult(LABEL_R1);
                Result r2 = CreateOrGetResult(LABEL_R2);
                // Change the valence of interactions to change the agent's motivation.
                AddOrGetPrimitiveInteraction(e1, r1, -1);
                AddOrGetPrimitiveInteraction(e1, r2, 1);
                AddOrGetPrimitiveInteraction(e2, r1, -1);
                AddOrGetPrimitiveInteraction(e2, r2, 1);
            }
            else
            {
                Experiment e1 = AddOrGetExperience(LABEL_E1);
                AddOrGetExperience(LABEL_E2);
                SetPreviousExperience(e1);
            }
        }

        /// <summary>
        /// Perform one step of a "stream of intelligence".
        /// </summary>
        public string Step()
        {
            switch (enhanced)
            {
                case false:
                    experience = GetPreviousExperience();
                    if (GetMood() == Mood.Bored | GetMood() == Mood.Pained)
                    {
                        experience = GetOtherExperience(experience);
                        SetSelfSatisfactionCounter(0);
                    }

                    Result anticipatedResult = Predict(experience);
                    Result result = ReturnResult010(experience);
                    Interaction enactedInteraction = AddOrGetPrimitiveInteraction(experience, result);

                    if (enactedInteraction.GetValence() >= 0)
                        SetMood(Mood.Pleased);
                    else
                        SetMood(Mood.Pained);

                    if (result == anticipatedResult)
                    {
                        SetMood(Mood.SelfSatisfied);
                        IncrementSelfSatisfactionCounter();
                    }
                    else
                    {
                        SetMood(Mood.Frustrated);
                        SetSelfSatisfactionCounter(0);
                    }
                    if (GetSelfSatisfactionCounter() >= BoredomLevel)
                        SetMood(Mood.Bored);

                    SetPreviousExperience(experience);

                    return experience.GetLabel() + result.GetLabel() + " " + GetMood();
                case true:
                    List<Anticipation> anticipations = Anticipate();
                    Interaction intendedInteraction = SelectInteraction(anticipations);
                    experience = intendedInteraction.GetExperience();
                    result = ReturnResult030(experience);
                    _enactedInteraction = GetInteraction(experience != null && result != null ? experience.GetLabel() + result.GetLabel() : null);

                    Interaction typedEnactedInteraction = (Interaction)_enactedInteraction;
                    args.Message = "Enacted " + typedEnactedInteraction;
                    OnMessagePassing(args);

                    if (typedEnactedInteraction.GetValence() >= 0)
                        SetMood(Mood.Pleased);
                    else
                        SetMood(Mood.Pained);

                    LearnCompositeInteraction(typedEnactedInteraction);
                    SetEnactedInteraction(typedEnactedInteraction);

                    return GetMood().ToString();
            }
        }

        #region Interaction objects
        /// <summary>
        /// Generates a list of anticipations based on activated interactions.
        /// </summary>
        /// <returns>A list of anticipations.</returns>
        public List<Anticipation> Anticipate()
        {
            List<Anticipation> anticipations = new List<Anticipation>();
            Interaction enacted = GetEnactedInteraction();

            if (enacted != null)
            {
                foreach (Interaction activatedInteraction in GetActivatedInteractions())
                {
                    if (!(activatedInteraction is Interaction))
                        continue;
                    Interaction proposedInteraction = activatedInteraction.GetPostInteraction() as Interaction;
                    if (proposedInteraction != null)
                    {
                        anticipations.Add(new Anticipation(proposedInteraction));
                        args.Message = "afforded " + proposedInteraction;
                        OnMessagePassing(args);
                    }
                }
            }
            return anticipations;
        }
        /// <summary>
        /// Selects an interaction from the given anticipations.
        /// </summary>
        /// <param name="anticipations">The list of anticipations.</param>
        /// <returns>The selected interaction.</returns>
        public Interaction SelectInteraction(List<Anticipation> anticipations)
        {
            if (anticipations == null)
                anticipations = new List<Anticipation>();

            anticipations.Sort();
            Interaction intendedInteraction;

            if (anticipations.Count > 0)
            {
                Anticipation firstAnticipation = anticipations[0];
                Interaction affordedInteraction = (Interaction)((Anticipation)firstAnticipation).GetInteraction();
                intendedInteraction = affordedInteraction.GetValence() >= 0 ? affordedInteraction : GetOtherInteraction(affordedInteraction);
            }
            else
                intendedInteraction = GetOtherInteraction(null);

            return (Interaction)intendedInteraction;
        }
        /// <summary>
        /// Learns composite interactions based on the enacted interaction.
        /// </summary>
        /// <param name="interaction">The enacted interaction.</param>
        public void LearnCompositeInteraction(Interaction interaction)
        {
            if (interaction == null)
                return;

            Interaction preInteraction = GetEnactedInteraction();
            if (preInteraction != null)
                AddOrGetCompositeInteraction(preInteraction, interaction);
        }
        /// <summary>
        /// Records an interaction in memory.
        /// </summary>
        /// <param name="label">The label of this interaction.</param>
        /// <returns>The interaction.</returns>
        protected Interaction AddOrGetInteraction(string label)
        {
            if (!Interactions.ContainsKey(label))
                Interactions.Add(label, CreateInteraction(label));
            return Interactions.ContainsKey(label) ? Interactions[label] : null;
        }
        /// <summary>
        /// Gets an alternative interaction different from the specified one.
        /// </summary>
        /// <param name="interaction">The interaction to avoid.</param>
        /// <returns>An alternative interaction, or null if none is available.</returns>
        public Interaction GetOtherInteraction(Interaction interaction)
        {
            if (Interactions.Count == 0)
                return null;

            Interaction otherInteraction = null;
            foreach (Interaction e in Interactions.Values)
            {
                if (interaction == null || (e.GetExperience() != null && e.GetExperience() != interaction.GetExperience()))
                {
                    otherInteraction = e;
                    break;
                }
            }
            return otherInteraction;
        }
        /// <summary>
        /// Create a primitive interaction as a tuple.
        /// </summary>
        /// <param name="experience">The experience.</param>
        /// <param name="result">The result.</param>
        /// <returns>A synthesized interaction.</returns>
        protected Interaction AddOrGetPrimitiveInteraction(Experiment experience, Result result)
        {
            Interaction interaction = AddOrGetInteraction(experience.GetLabel() + result.GetLabel());
            interaction.SetExperience(experience);
            interaction.SetResult(result);
            return interaction;
        }
        /// <summary>
        /// Create a primitive interaction as a tuple.
        /// </summary>
        /// <param name="experience">The experience.</param>
        /// <param name="result">The result.</param>
        /// <param name="valence">The interaction's valence.</param>
        /// <returns>A synthesized interaction.</returns>
        protected Interaction AddOrGetPrimitiveInteraction(Experiment experience, Result result, int valence)
        {
            string label = experience.GetLabel() + result.GetLabel();
            Interaction interaction = CreateInteraction(label);
            if (!Interactions.ContainsKey(label))
            {
                interaction.SetExperience(experience);
                interaction.SetResult(result);
                interaction.SetValence(valence);
                Interactions.Add(label, interaction);
            }
            interaction = Interactions[(label)];
            return interaction;
        }
        /// <summary>
        /// Gets or creates a composite interaction with the specified pre- and post-interactions.
        /// </summary>
        /// <param name="preInteraction">The pre-interaction.</param>
        /// <param name="postInteraction">The post-interaction.</param>
        /// <returns>The composite interaction, or null if inputs are invalid.</returns>
        public Interaction AddOrGetCompositeInteraction(Interaction preInteraction, Interaction postInteraction)
        {
            if (preInteraction == null || postInteraction == null)
                return null;

            int valence = preInteraction.GetValence() + postInteraction.GetValence();
            string label = preInteraction.GetLabel() + postInteraction.GetLabel();
            Interaction compositeInteraction = new Interaction(label);
            Interaction interaction = AddOrGetInteraction(label);

            compositeInteraction.SetPreInteraction(preInteraction);
            compositeInteraction.SetPostInteraction(postInteraction);
            compositeInteraction.SetValence(valence);
            args.Message = "learn " + compositeInteraction.GetLabel();
            OnMessagePassing(args);

            return compositeInteraction;
        }
        /// <summary>
        /// Gets the list of activated interactions based on the current context.
        /// </summary>
        /// <returns>A list of activated interactions.</returns>
        public virtual List<Interaction> GetActivatedInteractions()
        {
            List<Interaction> activatedInteractions = new List<Interaction>();
            Interaction enacted = GetEnactedInteraction();
            if (enacted != null)
            {
                foreach (Interaction activatedInteraction in Interactions.Values)
                {
                    if (activatedInteraction.GetPreInteraction() == enacted)
                        activatedInteractions.Add(activatedInteraction);
                }
            }
            return activatedInteractions;
        }
        /// <summary>
        /// Creates an interaction.
        /// </summary>
        /// <param name="label">The label.</param>
        /// <returns></returns>
        protected Interaction CreateInteraction(string label)
        {
            return new Interaction(label);
        }
        /// <summary>
        /// Finds an interaction from its label.
        /// </summary>
        /// <param name="label">The label of this interaction.</param>
        /// <returns>The interaction.</returns>
        protected Interaction GetInteraction(string label)
        {
            return (Interaction)Interactions[label];
        }
        /// <summary>
        /// Gets the currently enacted interaction.
        /// </summary>
        /// <returns>The enacted interaction, or null if none is set.</returns>
        protected Interaction GetEnactedInteraction()
        {
            return _enactedInteraction;
        }
        /// <summary>
        /// Sets the currently enacted interaction.
        /// </summary>
        /// <param name="enactedInteraction">The interaction to set.</param>
        protected void SetEnactedInteraction(Interaction enactedInteraction)
        {
            _enactedInteraction = enactedInteraction;
        }
        #endregion

        #region Experiment objects

        /// <summary>
        /// Creates a new experience from its label and stores it in memory.
        /// </summary>
        /// <param name="label">The experience's label</param>
        /// <returns>The experience.</returns>
        protected Experiment AddOrGetExperience(string label)
        {
            if (!Experiences.ContainsKey(label))
                Experiences.Add(label, CreateExperience(label));
            return Experiences.ContainsKey(label) ? Experiences[label] : null;
        }

        protected Experiment CreateExperience(string label)
        {
            return new Experiment(label);
        }
        /// <summary>
        /// Finds an experience different from that passed in parameter.
        /// </summary>
        /// <param name="experience">The undesired experience.</param>
        /// <returns>The other experience.</returns>
        protected Experiment GetOtherExperience(Experiment experience)
        {
            Experiment otherExperience = null;
            foreach (Experiment e in Experiences.Values)
            {
                if (e != experience)
                {
                    otherExperience = e;
                    break;
                }
            }
            return otherExperience;
        }

        #endregion

        #region Results to be returned
        /// <summary>
        /// Creates a new result from its label and stores it in memory.
        /// </summary>
        /// <param name="label">The result's label.</param>
        /// <returns>The result.</returns>
        protected Result CreateOrGetResult(string label)
        {
            if (!Results.ContainsKey(label))
                Results[label] = new Result(label);
            return Results[label];
        }
        /// <summary>
        /// Finds an interaction from its experience.
        /// </summary>
        /// <param name="experience">The experience.</param>
        /// <returns>The interaction.</returns>
        protected Result Predict(Experiment experience)
        {
            Interaction interaction = null;
            Result anticipatedResult = null;

            foreach (Interaction i in Interactions.Values)
                if (i.GetExperience().Equals(experience))
                    interaction = i;

            if (interaction != null)
                anticipatedResult = interaction.GetResult();

            return anticipatedResult;
        }
        /// <summary>
        /// The Environment010 
        /// * E1 results in R1.E2 results in R2.
        /// </summary>
        /// <param name="experience">The current experience.</param>
        /// <returns>The result of this experience.</returns>
        public Result ReturnResult010(Experiment experience)
        {
            if (experience.Equals(AddOrGetExperience(LABEL_E1)))
                return CreateOrGetResult(LABEL_R1);
            else
                return CreateOrGetResult(LABEL_R2);
        }
        /// <summary>
        /// Returns the result for the given experience.
        /// </summary>
        /// <param name="experience">The experience to evaluate.</param>
        /// <returns>The resulting interaction result, or null if the experience is invalid.</returns>
        protected Result ReturnResult030(Experiment experience)
        {
            if (experience == null)
                return null;

            Result result = GetPreviousExperience() == experience ? CreateOrGetResult(LABEL_R1) : CreateOrGetResult(LABEL_R2);
            SetPreviousExperience(experience);
            return result;
        }

        #endregion

        #region Methods

        public Mood GetMood()
        {
            return mood;
        }
        public void SetMood(Mood mood)
        {
            this.mood = mood;
        }
        public Experiment GetPreviousExperience()
        {
            return previousExperience;
        }
        public void SetPreviousExperience(Experiment previousExperience)
        {
            this.previousExperience = previousExperience;
        }
        public int GetSelfSatisfactionCounter()
        {
            return selfSatisfactionCounter;
        }
        public void SetSelfSatisfactionCounter(int selfSatisfactionCounter)
        {
            this.selfSatisfactionCounter = selfSatisfactionCounter;
        }
        public void IncrementSelfSatisfactionCounter()
        {
            selfSatisfactionCounter++;
        }

        #endregion

        #region Event
        protected virtual void OnMessagePassing(MessageDelegateEventArgs e)
        {
            MessageToPass.Invoke(this, e);
        }
        #endregion

    }
}
