using MySql.Data.MySqlClient;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using BancoDeDados.Models;
using System;
using System.Linq;
using System.IO;

namespace BancoDeDados.Services.DataBase
{
    public class DataBase : IDataBase
    {

        private readonly string connectionString;

        public DataBase(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        
        
        public List<Comentarios> GetComments(string postID)
        {
            List<Comentarios> data = new List<Comentarios>();

            using(MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                if(connection.State == ConnectionState.Open)
                {
                    MySqlCommand command = new MySqlCommand();
                    command.Connection = connection;
                    command.CommandText = $@"SELECT Users.Nome, Users.UserID, Users.ImagePath, Comentario.ComentarioID,
                    Comentario.Texto FROM Users, Publicacao, Comentario WHERE @postID = Comentario.PublicacaoID
                    && Users.UserID = Comentario.UserID";
                    command.Parameters.AddWithValue("@postID", postID);
                    MySqlDataReader reader = command.ExecuteReader();

                    if(reader.HasRows)
                    {
                        while(reader.Read())
                        {
                            Comentarios comentarios = new Comentarios();
                            comentarios.UserID = reader.GetString("UserID");
                            comentarios.UserName = reader.GetString("Nome");
                            comentarios.UserImage = reader.GetString("ImagePath");

                            comentarios.ComentarioID = reader.GetString("ComentarioID");
                            comentarios.Texto = reader.GetString("Texto");

                            data.Add(comentarios);

                        }
                        connection.Close();
                        return data;
                    }

                }
                connection.Close();
                return null;
            }
        }

        public bool DoComment(string userID, string postID, string text)
        {
            using(MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                if(connection.State == ConnectionState.Open)
                {
                    MySqlCommand command = new MySqlCommand();
                    command.Connection = connection;
                    command.CommandText = "INSERT INTO Comentario(UserID, PublicacaoID, Texto ) VALUES (@userID, @postID, @text)";
                    command.Parameters.AddWithValue("userID", userID);
                    command.Parameters.AddWithValue("postID", postID);
                    command.Parameters.AddWithValue("text", text);

                    int nRowsAffected = command.ExecuteNonQuery();

                    if(nRowsAffected == 1)
                    {
                        connection.Close();
                        return true;
                    }
                }
                connection.Close();
                return false;
            }
        }

        public List<Posts> GetPosts()
        {
            List<Posts> posts = new List<Posts>();

            using(MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                if(connection.State == ConnectionState.Open)
                {
                    MySqlCommand command = new MySqlCommand();
                    command.Connection = connection;
                    command.CommandText = $@"SELECT Publicacao.PublicacaoID, Publicacao.UserID, Publicacao.Imagem, 
                    Publicacao.Texto, Users.Nome, Users.ImagePath FROM Publicacao, Users WHERE Users.UserID = Publicacao.UserID;
";
                    MySqlDataReader reader = command.ExecuteReader();
                    if(reader.HasRows)
                    {
                        while(reader.Read())
                        {
                            int id = 0;
                            Posts data = new Posts();
                            if(Int32.TryParse(reader.GetString("UserID"), out id))
                            {
                                data.UserID = id;
                                id = 0;
                            }
                            
                            if(Int32.TryParse(reader.GetString("PublicacaoID"), out id))
                            {
                                data.PublicacaoID = id;
                                id = 0;
                            }
                            
                            data.UserName = reader.GetString("Nome");
                            data.UserImage = reader.GetString("ImagePath");
                            
                            if(!reader.IsDBNull(2))
                                data.ImagePath = reader.GetString("Imagem");
                            
                            if(!reader.IsDBNull(3))
                                data.Text = reader.GetString("Texto");

                            posts.Add(data);
                        }
                        
                        connection.Close();
                        var sort = from r in posts orderby Guid.NewGuid() ascending select r;
                        posts = sort.ToList();
                        return posts;
                    }
                }
                connection.Close();
                return null;
            }

        }

        public bool DoPost(string userID,string post, IFormFile image)
        {

            using(MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                if(connection.State == ConnectionState.Open)
                {
                    MySqlCommand command = new MySqlCommand();
                    command.Connection = connection;
                    command.CommandText = "INSERT INTO Publicacao (UserID, Imagem, Texto) VALUES (@UserID, @Imagem, @Texto)";
                    
                    command.Parameters.AddWithValue("UserID", userID);

                    if(!string.IsNullOrEmpty(post))
                        command.Parameters.AddWithValue("Texto", post);
                    
                    if(image != null)
                    {
                        
                        string fileName = DateTime.Now.ToString("yyyymmddMMsss") + "_" + userID + Path.GetExtension(image.FileName);
                        FileStream stream = new FileStream("wwwroot/images/" + fileName, FileMode.Create);
                        image.CopyTo(stream);
                        command.Parameters.AddWithValue("Imagem", "~/images/" + fileName);
                    }

                    int nRowsAffected = command.ExecuteNonQuery();

                    if(nRowsAffected == 1)
                    {
                        connection.Close();
                        return true;
                    }
                }
                connection.Close();
                return false;
            }
        }

        public Dictionary<string, string> SearchUserByID(string id)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            
            using(MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                
                if(connection.State == ConnectionState.Open)
                {
                    MySqlCommand command = new MySqlCommand();
                    command.Connection = connection;
                    command.CommandText = "SELECT Nome, City, ImagePath FROM Users WHERE UserID = @id LIMIT 1";
                    command.Parameters.AddWithValue("id", id);

                    MySqlDataReader reader = command.ExecuteReader();

                    if(reader.HasRows)
                    {
                        reader.Read();

                        data.Add("Nome", reader.GetString("Nome"));
                        data.Add("ImagePath", reader.GetString("ImagePath"));

                        if(reader.IsDBNull(1))
                            data.Add("City", "A Definir");
                        else
                            data.Add("City", reader.GetString("City"));

                        connection.Close();
                        return data;
                    }
                }
                connection.Close();
                return null;
            }
        }

        public List<Dictionary<string, string>> SearchForName(string userName)
        {
            List<Dictionary<string,string>> listaData = new List<Dictionary<string,string>>();

            using(MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                if(connection.State == ConnectionState.Open)
                {
                    MySqlCommand command = new MySqlCommand();
                    command.Connection = connection;
                    command.CommandText = "SELECT UserID, Nome, City, ImagePath FROM Users WHERE Nome = @userName";
                    command.Parameters.AddWithValue("userName", userName);

                    MySqlDataReader reader = command.ExecuteReader();

                    if(reader.HasRows)
                    {
                        while(reader.Read())
                        {
                            Dictionary<string,string> data   = new Dictionary<string, string>();

                            data.Add("UserID", reader.GetString("UserID"));
                            data.Add("Nome", reader.GetString("Nome"));
                            data.Add("ImagePath", reader.GetString("ImagePath"));

                            if(reader.IsDBNull(2))
                                data.Add("Cidade", "A Definir");
                            else
                                data.Add("Cidade", reader.GetString("City"));

                            listaData.Add(data);                           
                        }

                        connection.Close();
                        return listaData;
                    }
                }

                connection.Close();
                return null;
            }           
        }

        public bool AuthenticationLogin(HttpContext context, Login data)
        {
            var user = VerifyAuth(data.Email, data.Password);
            if(user != null)
            {
                
                var claims = new List<Claim>
                {
                    new Claim("UserID", user["UserID"]),
                    new Claim("Nome", user["Nome"]),
                    new Claim(ClaimTypes.Role, "User")    
                };
                
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                
                AuthenticationHttpContextExtensions.SignInAsync(context, CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));    

                return true;
            }
            return false;
        }

        public Dictionary<string,string> VerifyAuth(string email, string password)
        {
            
            using(MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                if(connection.State == ConnectionState.Open)
                {
                    MySqlCommand command = new MySqlCommand();
                    command.Connection = connection;
                    command.CommandText = $@"SELECT UserID, Nome FROM Users WHERE Email = @email && Pass = @password";

                    command.Parameters.AddWithValue("email", email);
                    command.Parameters.AddWithValue("password", password);

                    MySqlDataReader user = command.ExecuteReader();
                    
                    if(user.HasRows)
                    {   
                        user.Read();
                        Dictionary<string,string> data = new Dictionary<string,string>();
                        data.Add("UserID", user.GetString(0));
                        data.Add("Nome", user.GetString(1));

                        connection.Close();
                        return data;
                    }
                    
                }

                connection.Close();
                return null;
            }
            
        }

        public bool Register(Users user)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                if(connection.State == ConnectionState.Open && VerifyExistEmail(user.Email))
                {
                    MySqlCommand command = new MySqlCommand();
                    command.Connection = connection;
                    command.CommandText = "INSERT INTO Users(Nome, Email, Pass, ImagePath) VALUES (@Name, @Email, @Pass, @ImagePath)";

                    command.Parameters.AddWithValue("Name", user.Name);
                    command.Parameters.AddWithValue("Email", user.Email);
                    command.Parameters.AddWithValue("Pass", user.Password);
                    command.Parameters.AddWithValue("ImagePath", "~/images/user.png");

                    int nRowsAffected = command.ExecuteNonQuery();

                    if(nRowsAffected == 1)
                    {
                        connection.Close();
                        return true;
                    }
                }
                
                connection.Close();
                return false;
            }
        }

        public bool VerifyExistEmail(string email)
        {
            using(MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                if(connection.State == ConnectionState.Open)
                {
                    MySqlCommand command = new MySqlCommand();
                    command.Connection = connection;
                    command.CommandText = $@"SELECT Email FROM Users WHERE Email = @Email LIMIT 1";
                
                    command.Parameters.AddWithValue("Email", email);
                    
                    MySqlDataReader rows = command.ExecuteReader();

                    if(rows.HasRows)
                    {
                        connection.Close();
                        return false;
                    }
                }

                connection.Close();
                return true;

            }
        }
    }
}