﻿using System.ComponentModel.DataAnnotations.Schema;

namespace EmployerService.JSON.ResponseJSON
{
    public class VacancyResponse
    {
        public int Id { get; set; }
        public string Job_name { get; set; }
        public string Description { get; set; }
        public string City { get; set; }
        public int Employer_id { get; set; }
        public int Status_id { get; set; }
        public string Status_description {  get; set; }
        public int Experience_id { get; set; }
        public string Experience_desc { get; set; }
    }
}
