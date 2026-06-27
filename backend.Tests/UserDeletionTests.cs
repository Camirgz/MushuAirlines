using backend.Interfaces;
using backend.Services;
using Moq;

namespace backend.Tests;

[TestFixture]
public class DeleteUserTests
{
    private Mock<IUserListRepository> _repositoryMock = null!;
    private UserListService _service = null!;

    [SetUp]
    public void Setup()
    {
        _repositoryMock = new Mock<IUserListRepository>();
        _service = new UserListService(_repositoryMock.Object);
    }

    [Test]
    public void DeleteUser_WhenUserExists_ShouldReturnEmptyString()
    {
        _repositoryMock
            .Setup(r => r.DeleteUser(It.IsAny<int>()))
            .Returns(true);

        string result = _service.DeleteUser(1);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void DeleteUser_ShouldCallRepositoryExactlyOnce()
    {
        _repositoryMock
            .Setup(r => r.DeleteUser(It.IsAny<int>()))
            .Returns(true);

        _service.DeleteUser(1);

        _repositoryMock.Verify(r => r.DeleteUser(1), Times.Once);
    }

    [Test]
    public void DeleteUser_ShouldPassCorrectIdToRepository()
    {
        int expectedId = 42;
        _repositoryMock
            .Setup(r => r.DeleteUser(expectedId))
            .Returns(true);

        _service.DeleteUser(expectedId);

        _repositoryMock.Verify(r => r.DeleteUser(expectedId), Times.Once);
    }

    [Test]
    public void DeleteUser_WhenUserDoesNotExist_ShouldReturnErrorMessage()
    {
        _repositoryMock
            .Setup(r => r.DeleteUser(It.IsAny<int>()))
            .Returns(false);

        string result = _service.DeleteUser(999);

        Assert.That(result, Is.Not.Empty);
    }

    [Test]
    public void DeleteUser_WhenUserDoesNotExist_ShouldReturnUserNotFoundMessage()
    {
        _repositoryMock
            .Setup(r => r.DeleteUser(It.IsAny<int>()))
            .Returns(false);

        string result = _service.DeleteUser(999);

        Assert.That(result, Is.EqualTo("No se encontró el usuario que desea eliminar."));
    }

    [Test]
    public void DeleteUser_WhenUserIsLinkedToAirport_ShouldStillReturnEmptyString()
    {
        _repositoryMock
            .Setup(r => r.DeleteUser(It.IsAny<int>()))
            .Returns(true);

        string result = _service.DeleteUser(2);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void DeleteUser_WhenRepositoryThrows_ShouldReturnGenericErrorMessage()
    {
        _repositoryMock
            .Setup(r => r.DeleteUser(It.IsAny<int>()))
            .Throws(new Exception("fallo interno"));

        string result = _service.DeleteUser(1);

        Assert.That(result, Is.EqualTo("No se pudo eliminar el usuario."));
    }

    [Test]
    public void DeleteUser_WhenRepositoryThrows_ShouldNotPropagateException()
    {
        _repositoryMock
            .Setup(r => r.DeleteUser(It.IsAny<int>()))
            .Throws(new Exception("fallo interno"));

        Assert.DoesNotThrow(() => _service.DeleteUser(1));
    }

    [Test]
    public void DeleteUser_WhenSqlExceptionOccurs_ShouldReturnGenericErrorMessage()
    {
        _repositoryMock
            .Setup(r => r.DeleteUser(It.IsAny<int>()))
            .Throws(new InvalidOperationException("Violación de constraint."));

        string result = _service.DeleteUser(1);

        Assert.That(result, Is.EqualTo("No se pudo eliminar el usuario."));
    }

    [Test]
    public void DeleteUser_WithIdZero_ShouldReturnInvalidUserMessage()
    {
        string result = _service.DeleteUser(0);

        Assert.That(result, Is.EqualTo("El usuario seleccionado no es válido."));
    }

    [Test]
    public void DeleteUser_WithIdZero_ShouldNotCallRepository()
    {
        _service.DeleteUser(0);

        _repositoryMock.Verify(r => r.DeleteUser(It.IsAny<int>()), Times.Never);
    }

    [Test]
    public void DeleteUser_WithNegativeId_ShouldReturnInvalidUserMessage()
    {
        string result = _service.DeleteUser(-1);

        Assert.That(result, Is.EqualTo("El usuario seleccionado no es válido."));
    }

    [Test]
    public void DeleteUser_WithNegativeId_ShouldNotCallRepository()
    {
        _service.DeleteUser(-1);

        _repositoryMock.Verify(r => r.DeleteUser(It.IsAny<int>()), Times.Never);
    }
}