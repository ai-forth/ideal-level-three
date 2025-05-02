namespace ideal.coupling
{
    /// <summary>
    /// An interaction is the association of an experience with a result.
    /// </summary>
    public class Interaction
    {
        protected readonly string _label;
        protected Experiment _experience;
        protected Result _result;
        private int _valence;
        private Interaction _preInteraction;
        private Interaction _postInteraction;

        public Interaction(string label)
        {
            _label = label;
        }

        public string GetLabel()
        {
            return _label;
        }

        public Experiment GetExperience()
        {
            return _experience;
        }

        public void SetExperience(Experiment experience)
        {
            _experience = experience;
        }

        public Result GetResult()
        {
            return _result;
        }

        public void SetResult(Result result)
        {
            _result = result;
        }

        public int GetValence()
        {
            return _valence;
        }

        public void SetValence(int valence)
        {
            _valence = valence;
        }

        public void SetPreInteraction(Interaction preInteraction)
        {
            _preInteraction = preInteraction;
        }

        public Interaction GetPreInteraction()
        {
            return _preInteraction;
        }

        public void SetPostInteraction(Interaction postInteraction)
        {
            _postInteraction = postInteraction;
        }

        public Interaction GetPostInteraction()
        {
            return _postInteraction;
        }

        public bool IsPrimitive()
        {
            return GetPreInteraction() == null && GetPostInteraction() == null;
        }

        public override string ToString()
        {
            return _experience.GetLabel() + _result.GetLabel();
        }
    }
}
