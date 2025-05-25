//using Microsoft.AspNetCore.Mvc;
//using Moq;
//using SkillChallenge.Controllers;
//using SkillChallenge.Interfaces;
//using SkillChallenge.Models;

//namespace SkillChallengeUnitTests
//{
//    public class UserControllerTests
//    {
//        [Fact]
//        public async Task TestGetAllUsers()
//        {
//            // Arrange
//            var mockRepo = new Mock<IUserRepository>();
//            mockRepo
//                .Setup(r => r.GetAllUsersAsync())
//                .ReturnsAsync(
//                    new List<User>
//                    {
//                        new User
//                        {
//                            UserId = 1,
//                            UserName = "TestUser1",
//                            Password = "TestPassword1",
//                            ProfilePicture = "Pic1",
//                        },
//                        new User
//                        {
//                            UserId = 2,
//                            UserName = "TestUser2",
//                            Password = "TestPassword2",
//                            ProfilePicture = "Pic2",
//                        },
//                    }
//                );
//            var controller = new UserController(mockRepo.Object);

//            // Act
//            var result = await controller.GetAllUsers();

//            // Assert
//            var okResult = Assert.IsType<OkObjectResult>(result);
//            var users = Assert.IsType<List<User>>(okResult.Value);
//            Assert.Equal(2, users.Count);
//            Assert.Equal("TestUser1", users[0].UserName);
//        }
//    }
//}
