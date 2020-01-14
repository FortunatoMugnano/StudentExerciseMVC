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
    public class ExercisesController : Controller
    {
        private readonly IConfiguration _config;

        public ExercisesController(IConfiguration config)
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
        // GET: Exercises
        public ActionResult Index()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Exercise.Id as ExerciseID, Exercise.Name as ExerciseName, Exercise.Language as ExerciseLanguage FROM Exercise";
                   


                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Exercise> exercises = new List<Exercise>();

                    while (reader.Read())
                    {

                        int currentExerciseID = reader.GetInt32(reader.GetOrdinal("ExerciseID"));
                        Exercise newExercise = exercises.FirstOrDefault(e => e.Id == currentExerciseID);

                        string nameValue = reader.GetString(reader.GetOrdinal("ExerciseName"));
                        string languageValue = reader.GetString(reader.GetOrdinal("ExerciseLanguage"));

                        if (newExercise == null)
                        {
                            newExercise = new Exercise
                            {
                                Id = currentExerciseID,
                                Name = nameValue,
                                Language = languageValue


                            };

                        }

                        exercises.Add(newExercise);

                    }

                    reader.Close();

                    return View(exercises);
                }
            }

        }

        // GET: Exercises/Details/5
        public ActionResult Details(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT
                            Id, Name, Language
                        FROM Exercise
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Exercise exercise = null;

                    if (reader.Read())
                    {
                        int idValue = reader.GetInt32(reader.GetOrdinal("Id"));
                        string nameValue = reader.GetString(reader.GetOrdinal("Name"));
                        string languageValue = reader.GetString(reader.GetOrdinal("Language"));

                        exercise = new Exercise
                        {
                            Id = idValue,
                            Name = nameValue,
                            Language = languageValue

                        };


                    };
                    reader.Close();

                    if (exercise == null)
                    {
                        return NotFound($"No Exercise found with the id of {id}");
                    }
                    return View(exercise);
                }
            }
            
        }

        // GET: Exercises/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Exercises/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Exercise exercise)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"INSERT INTO Exercise (Name, Language) 
                                                VALUES (@name, @language)";

                        cmd.Parameters.Add(new SqlParameter("@name", exercise.Name));
                        cmd.Parameters.Add(new SqlParameter("@language", exercise.Language));


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

        // GET: Exercises/Edit/5
        public ActionResult Edit(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT exercise.id as ExerciseId, exercise.name as ExerciseName, exercise.language as ExerciseLanguage 
                                        FROM exercise
                                        WHERE exercise.id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();

                    Exercise exercise = null;

                    if (reader.Read())
                    {

                        exercise = new Exercise
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("ExerciseId")),
                            Name = reader.GetString(reader.GetOrdinal("ExerciseName")),
                            Language = reader.GetString(reader.GetOrdinal("ExerciseLanguage"))
                        };


                        reader.Close();

                        return View(exercise);

                    }

                    reader.Close();

                    if (exercise == null)
                    {
                        return NotFound($"No Exercise found with the id of {id}");
                    }
                    return View(exercise);

                }
            }
            
        }

        // POST: Exercises/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Exercise exercise)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Exercise
                                            SET Name = @name, Language = @language
                                            WHERE Id = @id";

                        cmd.Parameters.Add(new SqlParameter("@name", exercise.Name));
                        cmd.Parameters.Add(new SqlParameter("@language", exercise.Language));
                        cmd.Parameters.Add(new SqlParameter("@id", id));


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

        // GET: Exercises/Delete/5
        public ActionResult Delete(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT exercise.id as ExerciseId, exercise.name as ExerciseName, exercise.language as ExerciseLanguage 
                                        FROM exercise
                                        WHERE exercise.id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();

                    Exercise exercise = null;

                    if (reader.Read())
                    {

                        exercise = new Exercise
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("ExerciseId")),
                            Name = reader.GetString(reader.GetOrdinal("ExerciseName")),
                            Language = reader.GetString(reader.GetOrdinal("ExerciseLanguage"))
                        };


                        reader.Close();

                        return View(exercise);

                    }

                    reader.Close();

                    if (exercise == null)
                    {
                        return NotFound($"No Exercise found with the id of {id}");
                    }
                    return View(exercise);

                }
            }
           
        }

        // POST: Exercises/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, Exercise exercise)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE FROM Exercise WHERE Id = @id";

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