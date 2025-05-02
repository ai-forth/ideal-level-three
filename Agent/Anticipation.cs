using System;
using ideal.coupling;

namespace ideal.agent
{
    /// <summary>
    /// Base class for anticipations.
    /// </summary>
    public class Anticipation : IComparable<Anticipation>
    {
        private Experiment _experience;
        private Interaction _interaction;

        /// <summary>
        /// Initializes a new instance of the Anticipation class.
        /// </summary>
        /// <param name="interaction">The associated interaction.</param>
        public Anticipation(Interaction interaction)
        {
            interaction.GetExperience();
            if (interaction == null)
                throw new ArgumentNullException(interaction.ToString());
            _interaction = interaction;
        }
        /// <summary>
        /// Gets the interaction associated with the anticipation.
        /// </summary>
        /// <returns>The interaction.</returns>
        public Interaction GetInteraction()
        {
            return _interaction;
        }

        /// <summary>
        /// Compares this anticipation to another based on interaction valence.
        /// </summary>
        /// <param name="anticipation">The other anticipation.</param>
        /// <returns>A negative number if this is less, zero if equal, positive if greater.</returns>
        public int CompareTo(Anticipation anticipation)
        {
            if (anticipation == null)
                return 1;

            Anticipation other = anticipation;
            return other.GetInteraction().GetValence().CompareTo(_interaction.GetValence());
        }
        /// <summary>
        /// Gets the experience associated with the anticipation.
        /// </summary>
        /// <returns>The experience.</returns>
        public Experiment GetExperience()
        {
            return _experience;
        }
        /// <summary>
        /// Returns a string representation of the anticipation.
        /// </summary>
        /// <returns>A string describing the anticipation.</returns>
        public override string ToString()
        {
            return _interaction != null ? _interaction.ToString() : "null";
        }
    }
}
