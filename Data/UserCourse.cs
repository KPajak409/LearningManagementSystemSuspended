﻿namespace LMS.Data
{
    public class UserCourse
    {
        public string UserId { get; set; }
        public virtual User User { get; set; }
        public int CourseId { get; set; }
        public virtual Course Course { get; set; }
    }
}
