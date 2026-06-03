using backend.Interfaces;
using backend.Model;
using backend.Services;
using Moq;

namespace backend.Tests;

[TestFixture]
public class ProfileServiceTests
{
    private Mock<IProfileRepository> _mockProfileRepository = null!;
    private ProfileService _profileService = null!;

    [SetUp]
    public void Setup()
    {
        _mockProfileRepository = new Mock<IProfileRepository>();
        _profileService = new ProfileService(_mockProfileRepository.Object);
    }

    [Test]
    public void GetProfileByUsername_WhenUsernameIsWhiteSpace_ShouldReturnNull()
    {
        // Arrange
        string username = "   ";

        // Act
        var result = _profileService.GetProfileByUsername(username);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetProfileByUsername_WhenUsernameIsValid_ShouldReturnProfile()
    {
        // Arrange
        string username = " user@test.com ";
        string normalizedUsername = "user@test.com";

        var expectedProfile = new ProfileModel
        {
            FirstName = "Steven",
            LastName = "Mora",
            Ssn = "111111111",
            Nationality = "Costarricense",
            Email = "user@test.com",
            Salary = 1000,
            WorkSchedule = "L-V 8am a 12pm",
            Permissions = "Gestión",
            Role = "Operator"
        };

        _mockProfileRepository
            .Setup(repository => repository.GetProfileByUsername(normalizedUsername))
            .Returns(expectedProfile);

        // Act
        var result = _profileService.GetProfileByUsername(username);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.FirstName, Is.EqualTo(expectedProfile.FirstName));
            Assert.That(result.Email, Is.EqualTo(expectedProfile.Email));
        });
    }

    [Test]
    public void UpdateProfile_WhenUsernameIsWhiteSpace_ShouldReturnErrorMessage()
    {
        // Arrange
        var profile = new ProfileUpdateModel
        {
            FirstName = "Steven",
            LastName = "Mora",
            Ssn = "1234567890",
            Nationality = "Costarricense"
        };

        // Act
        var result = _profileService.UpdateProfile("   ", "Operator", profile);

        // Assert
        Assert.That(result, Is.EqualTo("No se pudo identificar el usuario conectado."));
    }

    [Test]
    public void UpdateProfile_WhenProfileIsNull_ShouldReturnErrorMessage()
    {
        // Act
        var result = _profileService.UpdateProfile("user@test.com", "Operator", null!);

        // Assert
        Assert.That(result, Is.EqualTo("Debe ingresar los datos del perfil."));
    }

    [Test]
    public void UpdateProfile_WhenOperatorFirstNameIsEmpty_ShouldReturnErrorMessage()
    {
        // Arrange
        string username = "user@test.com";

        var currentProfile = new ProfileModel
        {
            FirstName = "Steven",
            LastName = "Mora",
            Ssn = "111111111",
            Nationality = "Costarricense",
            Email = "user@test.com",
            Salary = 1000,
            WorkSchedule = "L-V 8am a 12pm",
            Permissions = "Gestión",
            Role = "Operator"
        };

        var profile = new ProfileUpdateModel
        {
            FirstName = "   ",
            LastName = "Mora",
            Ssn = "111111111",
            Nationality = "Costarricense"
        };

        _mockProfileRepository
            .Setup(repository => repository.GetProfileByUsername(username))
            .Returns(currentProfile);

        // Act
        var result = _profileService.UpdateProfile(username, "Operator", profile);

        // Assert
        Assert.That(result, Is.EqualTo("Debe ingresar el nombre."));
    }

    [Test]
    public void UpdateProfile_WhenOperatorLastNameIsEmpty_ShouldReturnErrorMessage()
    {
        // Arrange
        string username = "user@test.com";

        var currentProfile = new ProfileModel
        {
            FirstName = "Steven",
            LastName = "Mora",
            Ssn = "111111111",
            Nationality = "Costarricense",
            Email = "user@test.com",
            Salary = 1000,
            WorkSchedule = "L-V 8am a 12pm",
            Permissions = "Gestión",
            Role = "Operator"
        };

        var profile = new ProfileUpdateModel
        {
            FirstName = "Steven",
            LastName = "   ",
            Ssn = "111111111",
            Nationality = "Costarricense"
        };

        _mockProfileRepository
            .Setup(repository => repository.GetProfileByUsername(username))
            .Returns(currentProfile);

        // Act
        var result = _profileService.UpdateProfile(username, "Operator", profile);

        // Assert
        Assert.That(result, Is.EqualTo("Debe ingresar el apellido."));
    }

    [Test]
    public void UpdateProfile_WhenOperatorSsnIsEmpty_ShouldReturnErrorMessage()
    {
        // Arrange
        string username = "user@test.com";

        var currentProfile = new ProfileModel
        {
            FirstName = "Steven",
            LastName = "Mora",
            Ssn = "111111111",
            Nationality = "Costarricense",
            Email = "user@test.com",
            Salary = 1000,
            WorkSchedule = "L-V 8am a 12pm",
            Permissions = "Gestión",
            Role = "Operator"
        };

        var profile = new ProfileUpdateModel
        {
            FirstName = "Steven",
            LastName = "Mora",
            Ssn = "   ",
            Nationality = "Costarricense"
        };

        _mockProfileRepository
            .Setup(repository => repository.GetProfileByUsername(username))
            .Returns(currentProfile);

        // Act
        var result = _profileService.UpdateProfile(username, "Operator", profile);

        // Assert
        Assert.That(result, Is.EqualTo("Debe ingresar el SSN."));
    }

    [Test]
    public void UpdateProfile_WhenOperatorNationalityIsEmpty_ShouldReturnErrorMessage()
    {
        // Arrange
        string username = "user@test.com";

        var currentProfile = new ProfileModel
        {
            FirstName = "Steven",
            LastName = "Mora",
            Ssn = "111111111",
            Nationality = "Costarricense",
            Email = "user@test.com",
            Salary = 1000,
            WorkSchedule = "L-V 8am a 12pm",
            Permissions = "Gestión",
            Role = "Operator"
        };

        var profile = new ProfileUpdateModel
        {
            FirstName = "Steven",
            LastName = "Mora",
            Ssn = "111111111",
            Nationality = "   "
        };

        _mockProfileRepository
            .Setup(repository => repository.GetProfileByUsername(username))
            .Returns(currentProfile);

        // Act
        var result = _profileService.UpdateProfile(username, "Operator", profile);

        // Assert
        Assert.That(result, Is.EqualTo("Debe ingresar la nacionalidad."));
    }

    [Test]
    public void UpdateProfile_WhenAdminSalaryIsNull_ShouldReturnErrorMessage()
    {
        // Arrange
        string username = "admin@test.com";

        var currentProfile = new ProfileModel
        {
            FirstName = "Enrique",
            LastName = "Quirós",
            Ssn = "222222222",
            Nationality = "Costarricense",
            Email = "admin@test.com",
            Salary = 2000,
            WorkSchedule = "L-V 8am a 4pm",
            Permissions = "All",
            Role = "Administrator"
        };

        var profile = new ProfileUpdateModel
        {
            FirstName = "Enrique",
            LastName = "Quirós",
            Ssn = "222222222",
            Nationality = "Costarricense",
            Salary = null,
            WorkSchedule = "L-V 8am a 4pm",
            Permissions = "All",
            Role = "Administrator"
        };

        _mockProfileRepository
            .Setup(repository => repository.GetProfileByUsername(username))
            .Returns(currentProfile);

        // Act
        var result = _profileService.UpdateProfile(username, "Administrator", profile);

        // Assert
        Assert.That(result, Is.EqualTo("Debe ingresar el salario."));
    }

    [Test]
    public void UpdateProfile_WhenAdminSalaryIsNegative_ShouldReturnErrorMessage()
    {
        // Arrange
        string username = "admin@test.com";

        var currentProfile = new ProfileModel
        {
            FirstName = "Enrique",
            LastName = "Quirós",
            Ssn = "222222222",
            Nationality = "Costarricense",
            Email = "admin@test.com",
            Salary = 2000,
            WorkSchedule = "L-V 8am a 4pm",
            Permissions = "All",
            Role = "Administrator"
        };

        var profile = new ProfileUpdateModel
        {
            FirstName = "Enrique",
            LastName = "Quirós",
            Ssn = "123456789",
            Nationality = "Costarricense",
            Salary = -1,
            WorkSchedule = "L-V 8am a 4pm",
            Permissions = "All",
            Role = "Administrator"
        };

        _mockProfileRepository
            .Setup(repository => repository.GetProfileByUsername(username))
            .Returns(currentProfile);

        // Act
        var result = _profileService.UpdateProfile(username, "Administrator", profile);

        // Assert
        Assert.That(result, Is.EqualTo("El salario no puede ser negativo."));
    }

    [Test]
    public void UpdateProfile_WhenAdminWorkScheduleIsEmpty_ShouldReturnErrorMessage()
    {
        // Arrange
        string username = "admin@test.com";

        var currentProfile = new ProfileModel
        {
            FirstName = "Enrique",
            LastName = "Quirós",
            Ssn = "222222222",
            Nationality = "Costarricense",
            Email = "admin@test.com",
            Salary = 2000,
            WorkSchedule = "L-V 8am a 4pm",
            Permissions = "All",
            Role = "Administrator"
        };

        var profile = new ProfileUpdateModel
        {
            FirstName = "Enrique",
            LastName = "Quirós",
            Ssn = "222222222",
            Nationality = "Costarricense",
            Salary = 2000,
            WorkSchedule = "   ",
            Permissions = "All",
            Role = "Administrator"
        };

        _mockProfileRepository
            .Setup(repository => repository.GetProfileByUsername(username))
            .Returns(currentProfile);

        // Act
        var result = _profileService.UpdateProfile(username, "Administrator", profile);

        // Assert
        Assert.That(result, Is.EqualTo("Debe ingresar el horario."));
    }

    [Test]
    public void UpdateProfile_WhenAdminPermissionsAreEmpty_ShouldReturnErrorMessage()
    {
        // Arrange
        string username = "admin@test.com";

        var currentProfile = new ProfileModel
        {
            FirstName = "Enrique",
            LastName = "Quirós",
            Ssn = "222222222",
            Nationality = "Costarricense",
            Email = "admin@test.com",
            Salary = 2000,
            WorkSchedule = "L-V 8am a 4pm",
            Permissions = "All",
            Role = "Administrator"
        };

        var profile = new ProfileUpdateModel
        {
            FirstName = "Enrique",
            LastName = "Quirós",
            Ssn = "222222222",
            Nationality = "Costarricense",
            Salary = 3000,
            WorkSchedule = "L-V 8am a 4pm",
            Permissions = "   ",
            Role = "Administrator"
        };

        _mockProfileRepository
            .Setup(repository => repository.GetProfileByUsername(username))
            .Returns(currentProfile);

        // Act
        var result = _profileService.UpdateProfile(username, "Administrator", profile);

        // Assert
        Assert.That(result, Is.EqualTo("Debe ingresar los permisos."));
    }

    [Test]
    public void UpdateProfile_WhenAdminRoleIsInvalid_ShouldReturnErrorMessage()
    {
        // Arrange
        string username = "admin@test.com";

        var currentProfile = new ProfileModel
        {
            FirstName = "Enrique",
            LastName = "Quirós",
            Ssn = "222222222",
            Nationality = "Costarricense",
            Email = "admin@test.com",
            Salary = 2000,
            WorkSchedule = "L-V 8am a 4pm",
            Permissions = "All",
            Role = "Administrator"
        };

        var profile = new ProfileUpdateModel
        {
            FirstName = "Enrique",
            LastName = "Quirós",
            Ssn = "222222222",
            Nationality = "Costarricense",
            Salary = 3000,
            WorkSchedule = "L-V 8am a 4pm",
            Permissions = "All",
            Role = "Supervisor"
        };

        _mockProfileRepository
            .Setup(repository => repository.GetProfileByUsername(username))
            .Returns(currentProfile);

        // Act
        var result = _profileService.UpdateProfile(username, "Administrator", profile);

        // Assert
        Assert.That(result, Is.EqualTo("El rol seleccionado no es válido."));
    }
}
