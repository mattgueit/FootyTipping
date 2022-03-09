using FootyTipping.Server.Authorization;
using FootyTipping.Server.Data;
using FootyTipping.Server.Entitites;
using FootyTipping.Server.Models;
using FootyTipping.Server.Services;
using AutoMapper;
using Moq;
using Moq.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System;

namespace FootyTipping.Server.Tests
{
    public class UserServiceTests
    {
        private readonly Mock<IHashingUtilities> _hashingUtilitiesMock;
        private readonly Mock<IJwtUtilities> _jwtUtilitiesMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<DataContext> _dataContextMock;

        public UserServiceTests()
        {
            _hashingUtilitiesMock = new Mock<IHashingUtilities>();
            _jwtUtilitiesMock = new Mock<IJwtUtilities>();
            _mapperMock = new Mock<IMapper>();
            _dataContextMock = new Mock<DataContext>();

            DataSetup();
        }

        private void DataSetup()
        {
            var users = new List<User>()
            {
                new User { Id = 1, FirstName = "Vikram",    LastName = "Barn",      Username = "Vikkstar123",   PasswordHash = "123" },
                new User { Id = 2, FirstName = "Ethan",     LastName = "Payne",     Username = "Behzinga",      PasswordHash = "345" },
                new User { Id = 3, FirstName = "Harry",     LastName = "Lewis",     Username = "Wroetoshaw",    PasswordHash = "678" },
                new User { Id = 4, FirstName = "Olajide",   LastName = "Olatunji",  Username = "KSIOlajideBT",  PasswordHash = "901" },
                new User { Id = 5, FirstName = "Joshua",    LastName = "Bradley",   Username = "Zerkaa",        PasswordHash = "456" },
                new User { Id = 6, FirstName = "Tobi",      LastName = "Brown",     Username = "TBJZL",         PasswordHash = "012" },
                new User { Id = 7, FirstName = "Simon",     LastName = "Minter",    Username = "Miniminter",    PasswordHash = "789" }
            };

            var mockSet = new Mock<DbSet<User>>();

            _dataContextMock.Setup(x => x.Users).ReturnsDbSet(users);
        }

        private UserService CreateUserService()
        {
            return new UserService(_dataContextMock.Object, _hashingUtilitiesMock.Object, _jwtUtilitiesMock.Object, _mapperMock.Object);
        }

        [Fact]
        public void Authenticate_ThrowException_WhenUserNotFoundInDatabase()
        {
            // Arrange
            var request = new AuthenticateRequest()
            {
                Username = "NonExistentUser",
                Password = "abc"
            };

            var service = CreateUserService();

            // Act & Assert
            Assert.Throws<Exception>(() => service.Authenticate(request));
        }

        [Fact]
        public void Authenticate_ThrowException_WhenPasswordIsIncorrect()
        {
            // Arrange
            var request = new AuthenticateRequest()
            {
                Username = "Vikkstar123",
                Password = "abc"
            };

            _hashingUtilitiesMock
                .Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(false);

            var service = CreateUserService();

            // Act & Assert
            Assert.Throws<Exception>(() => service.Authenticate(request));
        }

        [Fact]
        public void Authenticate_MapUserToResponse_WhenAuthenticationPasses()
        {
            // Arrange
            var request = new AuthenticateRequest()
            {
                Username = "Vikkstar123",
                Password = "abc"
            };

            _hashingUtilitiesMock
                .Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            _mapperMock.Setup(x => x.Map<AuthenticateResponse>(It.IsAny<User>())).Returns(new AuthenticateResponse());

            var service = CreateUserService();

            // Act
            var response = service.Authenticate(request);

            // Assert
            _mapperMock.Verify(x => x.Map<AuthenticateResponse>(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public void Authenticate_GenerateTokenForResponse_WhenAuthenticationPasses()
        {
            // Arrange
            var request = new AuthenticateRequest()
            {
                Username = "Vikkstar123",
                Password = "abc"
            };

            _hashingUtilitiesMock
                .Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            _mapperMock.Setup(x => x.Map<AuthenticateResponse>(It.IsAny<User>())).Returns(new AuthenticateResponse());

            var expectedToken = "Token";
            _jwtUtilitiesMock.Setup(x => x.GenerateToken(It.IsAny<User>())).Returns(expectedToken);

            var service = CreateUserService();

            // Act
            var response = service.Authenticate(request);

            // Assert
            Assert.Equal(expectedToken, response.Token);
        }

        [Fact]
        public void Register_WhenUsernameIsAlreadyTaken_ThrowApplicationException()
        {
            // Arrange
            var request = new RegisterRequest()
            {
                FirstName = "First",
                LastName = "Last",
                Username = "Vikkstar123",
                Password = "abc"
            };

            var service = CreateUserService();

            // Act & Assert
            Assert.Throws<ApplicationException>(() => service.Register(request));
        }

        [Fact]
        public void Register_WhenUsernameIsNotTaken_AddNewUser()
        {
            // Arrange
            var request = new RegisterRequest()
            {
                FirstName = "First",
                LastName = "Last",
                Username = "MasterDranzer",
                Password = "abc"
            };

            var user = new User()
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Username = request.Username,
            };

            _mapperMock
                .Setup(x => x.Map<User>(It.IsAny<RegisterRequest>()))
                .Returns(user);

            _hashingUtilitiesMock
                .Setup(x => x.HashPassword(It.IsAny<string>()))
                .Returns("abcabc");

            var service = CreateUserService();

            // Act
            service.Register(request);

            // Assert
            _dataContextMock.Verify(x => x.Users.Add(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public void Update_WhenNewUsernameIsTaken_ThrowApplicationException()
        {
            // Arrange
            var userId = 1;

            var request = new UpdateRequest()
            {
                FirstName = "Vik",
                LastName = "Barn",
                Username = "Wroetoshaw",  // somebody elses username
                Password = "abc"
            };

            _dataContextMock
                .Setup(x => x.Users.Find(It.IsAny<int>()))
                .Returns(new User() { Username = "Vikkstar123" });

            var service = CreateUserService();

            // Act & Assert
            Assert.Throws<ApplicationException>(() => service.Update(userId, request));
        }

        [Fact]
        public void Update_WhenUserIdDoesntExist_ThrowKeyNotFoundException()
        {
            // Arrange
            var userId = 1;

            var request = new UpdateRequest()
            {
                FirstName = "Vik",
                LastName = "Barn",
                Username = "Wroetoshaw",  // somebody elses username
                Password = "abc"
            };

            _dataContextMock
                .Setup(x => x.Users.Find(It.IsAny<int>()))
                .Returns((User) null);

            var service = CreateUserService();

            // Act & Assert
            Assert.Throws<KeyNotFoundException>(() => service.Update(userId, request));
        }

        [Fact]
        public void Update_WhenNewUserIsNotTaken_UpdateUser()
        {
            // Arrange
            var userId = 1;

            var request = new UpdateRequest()
            {
                FirstName = "Vik",
                LastName = "Barn",
                Username = "Vikkstar123HD",
                Password = "abc"
            };

            _dataContextMock
                .Setup(x => x.Users.Find(It.IsAny<int>()))
                .Returns(new User() { Username = "Vikkstar123" });

            var service = CreateUserService();

            // Act
            service.Update(userId, request);

            // Assert
            _dataContextMock.Verify(x => x.Users.Update(It.IsAny<User>()), Times.Once);
            _dataContextMock.Verify(x => x.SaveChanges(), Times.Once);
        }

    }
}