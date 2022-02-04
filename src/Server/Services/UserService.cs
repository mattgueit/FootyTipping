﻿using FootyTipping.Server.Authorization;
using FootyTipping.Server.Data;
using FootyTipping.Server.Entitites;
using FootyTipping.Server.Models;
using AutoMapper;

namespace FootyTipping.Server.Services
{
    public interface IUserService
    {
        AuthenticateResponse Authenticate(AuthenticateRequest request);
        IEnumerable<User> GetAll();
        User GetById(int id);
        void Register(RegisterRequest request);
        void Update(int id, UpdateRequest request);
        void Delete(int id);
    }

    public class UserService : IUserService
    {
        // TODO: check if these can be readonly? surely right?
        private DataContext _dataContext;
        private IJwtUtilities _jwtUtilities;
        private readonly IMapper _mapper;

        public UserService(DataContext dataContext, IJwtUtilities jwtUtilities, IMapper mapper)
        {
            _dataContext = dataContext;
            _jwtUtilities = jwtUtilities;
            _mapper = mapper;
        }

        public AuthenticateResponse Authenticate(AuthenticateRequest request)
        {
            // Fetch user from DB
            var user = _dataContext.Users.SingleOrDefault(x => x.Username == request.Username);

            // Validate
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                throw new Exception("Username or password is incorrect.");

            var response = _mapper.Map<AuthenticateResponse>(user);
            response.Token = _jwtUtilities.GenerateToken(user);
            return response;
        }

        public void Delete(int id)
        {
            var user = getUser(id);
            _dataContext.Users.Remove(user);
            _dataContext.SaveChanges();
        }

        public IEnumerable<User> GetAll()
        {
            return _dataContext.Users;
        }

        public User GetById(int id)
        {
            return getUser(id);
        }

        public void Register(RegisterRequest request)
        {
            // Validate
            if (_dataContext.Users.Any(x => x.Username == request.Username))
                throw new ApplicationException($"Username {request.Username} is already taken.");

            // Map model to new User object
            var user = _mapper.Map<User>(request);

            // Hash password if entered
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Save user to DB
            _dataContext.Users.Add(user);
            _dataContext.SaveChanges();
        }

        public void Update(int id, UpdateRequest request)
        {
            var user = getUser(id);

            // Validate
            if (request.Username != user.Username && _dataContext.Users.Any(x => x.Username == request.Username))
                throw new ApplicationException($"Username {request.Username} is already taken.");

            // Hash password if it was entered
            if (!string.IsNullOrEmpty(request.Password))
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Map request to user and save to DB
            _mapper.Map(request, user);
            _dataContext.Users.Update(user);
            _dataContext.SaveChanges();
        }

        private User getUser(int id)
        {
            var user = _dataContext.Users.Find(id);
            if (user == null)
                throw new KeyNotFoundException("User not found.");
            return user;
        }
    }
}
