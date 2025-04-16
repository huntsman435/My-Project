using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

[Route("api/users")]
[ApiController]
public class UsersController : ControllerBase
{
    private static List<User> users = new List<User>();

    // GET: Retrieve all users
    [HttpGet]
    public IActionResult GetAllUsers()
    {
        try
        {
            return Ok(users);
        }
        catch (Exception ex)
        {
            // Return a 500 error with details if something goes wrong
            return StatusCode(500, new { message = "An error occurred while retrieving users.", details = ex.Message });
        }
    }

    // GET: Retrieve a specific user by ID
    [HttpGet("{id}")]
    public IActionResult GetUserById(int id)
    {
        try
        {
            var user = users.FirstOrDefault(u => u.Id == id);
            if (user == null)
                return NotFound(new { message = "User not found" });
            return Ok(user);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving the user.", details = ex.Message });
        }
    }

    // POST: Add a new user with validation
    [HttpPost]
    public IActionResult CreateUser([FromBody] User user)
    {
        try
        {
            // Validate input
            if (user == null)
                return BadRequest(new { message = "User data is required." });
            if (string.IsNullOrWhiteSpace(user.Name))
                return BadRequest(new { message = "Name is required." });
            if (string.IsNullOrWhiteSpace(user.Email))
                return BadRequest(new { message = "Email is required." });
            if (!new EmailAddressAttribute().IsValid(user.Email))
                return BadRequest(new { message = "Invalid email format." });

            users.Add(user);
            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while creating the user.", details = ex.Message });
        }
    }

    // PUT: Update an existing user’s details
    [HttpPut("{id}")]
    public IActionResult UpdateUser(int id, [FromBody] User updatedUser)
    {
        try
        {
            var user = users.FirstOrDefault(u => u.Id == id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            // Validate input
            if (updatedUser == null)
                return BadRequest(new { message = "Updated user data is required." });
            if (string.IsNullOrWhiteSpace(updatedUser.Name))
                return BadRequest(new { message = "Name is required." });
            if (string.IsNullOrWhiteSpace(updatedUser.Email))
                return BadRequest(new { message = "Email is required." });
            if (!new EmailAddressAttribute().IsValid(updatedUser.Email))
                return BadRequest(new { message = "Invalid email format." });

            user.Name = updatedUser.Name;
            user.Email = updatedUser.Email;
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while updating the user.", details = ex.Message });
        }
    }

    // DELETE: Remove a user by ID
    [HttpDelete("{id}")]
    public IActionResult DeleteUser(int id)
    {
        try
        {
            var user = users.FirstOrDefault(u => u.Id == id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            users.Remove(user);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while deleting the user.", details = ex.Message });
        }
    }
}