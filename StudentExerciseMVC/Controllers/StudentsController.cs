using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using StudentExerciseMVC.Models;
using StudentExerciseMVC.Models.ViewModels;

namespace StudentExerciseMVC.Controllers
{
    public class StudentsController : Controller
    {
        private readonly IConfiguration _config;

        public StudentsController(IConfiguration config)
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
        // GET: Students
        public ActionResult Index()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                                        SELECT s.Id,
                                        s.FirstName,
                                        s.LastName,
                                        s.SlackHandle,
                                        s.CohortId,
                                        c.Name as CohortName
                                        FROM Student s
                                        LEFT JOIN Cohort c On s.CohortId = c.Id";
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Student> students = new List<Student>();
                    while (reader.Read())
                    {
                        Student student = new Student
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),

                        };



                        var hasCohort = !reader.IsDBNull(reader.GetOrdinal("CohortId"));

                        if (hasCohort)
                        {
                            student.Cohort = new Cohort()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                Name = reader.GetString(reader.GetOrdinal("CohortName"))

                            };
                        }

                        students.Add(student);
                    }

                    reader.Close();

                    return View(students);
                }
            }

        }

        // GET: Student/Details/5
        public ActionResult Details(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT s.Id AS StudentId, s.FirstName, s.LastName, s.SlackHandle, s.CohortId, c.Name AS CohortName 
                                        FROM Student s 
                                        LEFT JOIN Cohort c ON s.CohortId = c.Id
                                        WHERE s.Id = @Id";
                    cmd.Parameters.Add(new SqlParameter("@Id", id));
                    var reader = cmd.ExecuteReader();

                    Student student = null;

                    while (reader.Read())
                    {
                        if (student == null)
                        {
                            student = new Student
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("StudentId")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                                CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                Cohort = new Cohort
                                {
                                    Name = reader.GetString(reader.GetOrdinal("CohortName"))
                                }
                            };
                        }
                        student.Exercises = GetAllExercisesByStudentId(id);
                    }

                    reader.Close();

                    if (student == null)
                    {
                        return NotFound($"No Student found with the id of {id}");
                    }
                    return View(student);

                }
            }
        }

        // GET: Students/Create
        public ActionResult Create()
        {
            var cohorts = GetCohorts().Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.Id.ToString()
            }).ToList();

            var viewModel = new StudentViewModel
            {
                Student = new Student(),
                Cohorts = cohorts
            };

            return View(viewModel);
        }

        // POST: Students/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Student student)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"INSERT INTO Student (FirstName, LastName, CohortId, SlackHandle) 
                                            VALUES (@firstName, @lastName, @cohortId, @slkHandle)";

                        cmd.Parameters.Add(new SqlParameter("@firstName", student.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@lastName", student.LastName));
                        cmd.Parameters.Add(new SqlParameter("@cohortId", student.CohortId));
                        cmd.Parameters.Add(new SqlParameter("@slkHandle", student.SlackHandle));

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

        // GET: Students/Edit/5
        public ActionResult Edit(int id)
        {
            var cohorts = GetCohorts().Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.Id.ToString()
            }).ToList();
            var exercises = GetAllExercises().Select(e => new SelectListItem
            {
                Text = e.Name,
                Value = e.Id.ToString()
            }).ToList();

            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT s.Id, s.FirstName, s.LastName, s.SlackHandle, s.CohortId as StudentCohortId, c.Id as CohortId
                        FROM Student s
                        LEFT JOIN Cohort c on c.id = s.CohortId
                        WHERE s.Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Student student = null;

                    if (reader.Read())
                    {

                        student = new Student
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId"))

                        };

                        reader.Close();

                        var viewModel = new StudentViewModel
                        {
                            Student = student,
                            Cohorts = cohorts,
                            Exercises = exercises
                        };

                        return View(viewModel);


                    };
                    reader.Close();

                    if (student == null)
                    {
                        return NotFound($"No Student found with the id of {id}");
                    }
                    return View(student);
                }
            }

        }

        // POST: Students/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Student student)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Student
                                            SET FirstName = @firstName,
                                                LastName = @lastName, 
                                                SlackHandle = @slkhandle,
                                                CohortId = @cohortId
                                            WHERE Id = @id";

                        cmd.Parameters.Add(new SqlParameter("@firstName", student.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@lastName", student.LastName));
                        cmd.Parameters.Add(new SqlParameter("@slkhandle", student.SlackHandle));
                        cmd.Parameters.Add(new SqlParameter("@cohortId", student.CohortId));
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        cmd.ExecuteNonQuery();
                    }

                    DeleteAssignedExercises(student.Id);
                    AddStudentExercise(student.Id, student.ExerciseIds);
                    return RedirectToAction(nameof(Index));
                }

                
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Edit), new { Id = id });
            }
        }

        // GET: Students/Delete/5
        public ActionResult Delete(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT s.Id, s.FirstName, s.LastName, s.SlackHandle, s.CohortId as StudentCohortId, c.Id as CohortId, c.Name
                                        FROM Student s
                                        LEFT JOIN Cohort c on c.id = s.CohortId
                                        WHERE s.Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    var reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        var student = new Student
                        {
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            Cohort = new Cohort
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                Name = reader.GetString(reader.GetOrdinal("Name"))
                            }
                        };
                        reader.Close();
                        return View(student);
                    }

                    return NotFound();
                }
            }

        }

        // POST: Students/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete([FromRoute]int id, Student student)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE FROM Student WHERE Id = @id";

                        cmd.Parameters.Add(new SqlParameter("@Id", id));

                        cmd.ExecuteNonQuery();

                        return RedirectToAction(nameof(Index));
                    }
                }

            }
            catch (Exception ex)
            {
                return View();
            }
        }

        private List<Cohort> GetCohorts()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, Name FROM Cohort";

                    var reader = cmd.ExecuteReader();

                    var cohorts = new List<Cohort>();

                    while (reader.Read())
                    {
                        cohorts.Add(new Cohort
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name"))
                        });


                    }

                    reader.Close();

                    return cohorts;
                }
            }
        }

        private List<Exercise> GetAllExercises()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, Name, Language 
                                       FROM Exercise";

                    var reader = cmd.ExecuteReader();

                    var exercises = new List<Exercise>();

                    while (reader.Read())
                    {
                        exercises.Add(new Exercise
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Language = reader.GetString(reader.GetOrdinal("Language"))
                        });
                    }

                    reader.Close();

                    return exercises;
                }
            }
        }

        private List<Exercise> GetAllExercisesByStudentId(int studentId)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT e.Id AS ExerciseId, e.Name AS Exercise, e.Language
                                        FROM Exercise e 
                                        INNER JOIN StudentExercise se ON e.Id = se.ExerciseId
                                        WHERE StudentId = @StudentId";

                    cmd.Parameters.AddWithValue("@StudentId", studentId);

                    var reader = cmd.ExecuteReader();

                    List<Exercise> exercises = new List<Exercise>();

                    while (reader.Read())
                    {
                        Exercise exercise = new Exercise
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("ExerciseId")),
                            Name = reader.GetString(reader.GetOrdinal("Exercise")),
                            Language = reader.GetString(reader.GetOrdinal("Language")),
                        };
                        exercises.Add(exercise);

                    };
                    reader.Close();
                    return exercises;
                }
            }
        }

        private void AddStudentExercise(int studentId, List<int> exerciseIdS)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                foreach (var exerciseId in exerciseIdS)
                {
                    using (SqlCommand cmd = conn.CreateCommand())
                    {

                        cmd.CommandText = @"INSERT INTO StudentExercise(StudentId, ExerciseId) 
                                            VALUES(@StudentId, @ExerciseId)";

                        cmd.Parameters.AddWithValue("@StudentId", studentId);
                        cmd.Parameters.AddWithValue("@ExerciseId", exerciseId);

                        cmd.ExecuteNonQuery();

                    }
                }

            }

        }

        private void DeleteAssignedExercises(int studentId)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE FROM StudentExercise 
                                            WHERE StudentId = @studentId";

                        cmd.Parameters.Add(new SqlParameter("@studentId", studentId));

                        cmd.ExecuteNonQuery();

                    }
                }

            }
            catch
            {
                throw;
            }
        }
    }
}