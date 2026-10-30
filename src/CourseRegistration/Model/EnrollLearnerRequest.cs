using System.ComponentModel.DataAnnotations;

namespace CourseRegistration.Model;
 
/// <summary>
      /// Request model for enrolling a learner in the default offering.
  /// </summary>
 public sealed record EnrollLearnerRequest
 {
      /// <summary>
        /// Gets the learner ID.
      /// </summary>
     [Range(1, int.MaxValue)]
     public int LearnerId { get; init; }
 }

