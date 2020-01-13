using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using StudentExerciseMVC.Models;

namespace StudentExerciseMVC.Controllers
{
    public class CohortsController : Controller
    {
        private readonly IConfiguration _config;

        public CohortsController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }
        // GET: Cohorts
        public ActionResult Index()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT cohort.id as CohortID, cohort.name as CohortName 
                                        FROM cohort";
                    
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Cohort> cohorts = new List<Cohort>();
                    while (reader.Read())
                    {
                        int currentCohortID = reader.GetInt32(reader.GetOrdinal("CohortID"));
                        Cohort newCohort = cohorts.FirstOrDefault(i => i.Id == currentCohortID);
                        if (newCohort == null)
                        {
                            newCohort = new Cohort
                            {
                                Id = currentCohortID,
                                Name = reader.GetString(reader.GetOrdinal("CohortName")),
                                Students = new List<Student>(),
                                Instructors = new List<Instructor>()
                            };
                            cohorts.Add(newCohort);
                        }

                      
                    }

                    reader.Close();

                    return View(cohorts);
                }
            }
           
        }

        // GET: Cohorts/Details/5
        public ActionResult Details(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT DISTINCT cohort.id as CohortID, cohort.name as CohortName,
                                        i.id as InstructorId, i.FirstName as InstructorFirst, i.LastName as InstructorLast, i.slackHandle as InstructorSlack, i.Speciality, i.cohortId as InstructorCohortID, 
                                        s.id as StudentId, s.FirstName as StudentFirst, s.LastName as StudentLast, s.SlackHandle as StudentSlack, s.cohortId as StudentCohortID 
                                        FROM cohort
                                        LEFT JOIN student as s ON cohort.id = s.cohortId  
                                        LEFT JOIN instructor as i ON cohort.id = i.cohortId
                                        WHERE cohort.id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Cohort newCohort = null;
                    while (reader.Read())
                    {

                        if (newCohort == null)
                        {
                            newCohort = new Cohort
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("CohortID")),
                                Name = reader.GetString(reader.GetOrdinal("CohortName")),

                            };
                        }


                        var hasStudent = !reader.IsDBNull(reader.GetOrdinal("StudentId"));
                        
                       if (hasStudent)
                       {
                            var studentId = reader.GetInt32(reader.GetOrdinal("StudentId"));
                            var studentAdded =  newCohort.Students.Any(s => s.Id == studentId);
                            if (!studentAdded)
                            {
                                newCohort.Students.Add(new Student
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("StudentId")),
                                    FirstName = reader.GetString(reader.GetOrdinal("StudentFirst")),
                                    LastName = reader.GetString(reader.GetOrdinal("StudentLast")),
                                    SlackHandle = reader.GetString(reader.GetOrdinal("StudentSlack"))

                                });
                            }
                       }


                       var hasInstructor = !reader.IsDBNull(reader.GetOrdinal("InstructorID"));
                      
                            if (hasInstructor)
                            {

                               var instructorId = reader.GetInt32(reader.GetOrdinal("InstructorID"));
                               var instructorAdded = newCohort.Instructors.Any(i => i.Id == instructorId);

                               if (!instructorAdded)
                               {
                                   newCohort.Instructors.Add(new Instructor
                                   {
                                    Id = reader.GetInt32(reader.GetOrdinal("InstructorID")),
                                    FirstName = reader.GetString(reader.GetOrdinal("InstructorFirst")),
                                    LastName = reader.GetString(reader.GetOrdinal("InstructorLast")),
                                    SlackHandle = reader.GetString(reader.GetOrdinal("InstructorSlack")),
                                    Specialty = reader.GetString(reader.GetOrdinal("Speciality"))

                                   });
                               }
                            }
                        

                       
                    }
                    reader.Close();

                    if (newCohort == null)
                    {
                        return NotFound();
                    }

                    return View(newCohort);
                    

                   
                }
            }
           
        }

        // GET: Cohorts/Create
        public ActionResult Create()
        {
            return View();
            
        }

        // POST: Cohorts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Cohort cohort)
        {


            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"INSERT INTO Cohort (Name) 
                                                VALUES (@name)";

                        cmd.Parameters.Add(new SqlParameter("@name", cohort.Name));


                        cmd.ExecuteNonQuery();
                    }
                }

                return RedirectToAction(nameof(Index));


            }

            catch
            {
                return View();
            }
        }
        

        // GET: Cohorts/Edit/5
        public ActionResult Edit(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT cohort.id as CohortID, cohort.name as CohortName 
                                        FROM cohort
                                        WHERE cohort.id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();

                    Cohort cohort = null;

                    if (reader.Read())
                    {

                        cohort = new Cohort
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                            Name = reader.GetString(reader.GetOrdinal("CohortName"))
                        };


                        reader.Close();

                        return View(cohort);

                    }

                    reader.Close();

                    if (cohort == null)
                    {
                        return NotFound($"No Cohort found with the id of {id}");
                    }
                    return View(cohort);

                }
            }
            
        }

        // POST: Cohorts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Cohort cohort)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Cohort
                                            SET Name = @name
                                            WHERE Id = @id";

                        cmd.Parameters.Add(new SqlParameter("@name", cohort.Name));
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        cmd.ExecuteNonQuery();
                    }

                    return RedirectToAction(nameof(Index));
                }
            }
            catch
            {
                return View();
            }
        }

        // GET: Cohorts/Delete/5
        public ActionResult Delete(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT cohort.id as CohortID, cohort.name as CohortName 
                                        FROM cohort
                                        WHERE cohort.id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();

                    Cohort cohort = null;

                    if (reader.Read())
                    {

                        cohort = new Cohort
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                            Name = reader.GetString(reader.GetOrdinal("CohortName"))
                        };


                        reader.Close();

                        return View(cohort);

                    }

                    reader.Close();

                    if (cohort == null)
                    {
                        return NotFound($"No Cohort found with the id of {id}");
                    }
                    return View(cohort);

                }
            }
            
        }

        // POST: Cohorts/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE FROM Cohort WHERE Id = @id";

                        cmd.Parameters.Add(new SqlParameter("@Id", id));

                        cmd.ExecuteNonQuery();

                        return RedirectToAction(nameof(Index));
                    }
                }

            }
            catch
            {
                return View();
            }
        }
    }
}