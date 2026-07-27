using CarvedRock.Core;
using CarvedRock.Domain;

namespace CarvedRock.Tests;

public class UnitTests
{
    [Test]
    public async Task NameIsRequiredProductValidation()
    {
        var validator = new NewProductValidator();

        var newProduct = new NewProductModel
        {
            Category = "boots",
            Description = "really nice footwear - you'll love them!",
            ImgUrl = "https://some.place/image.png",
            Price = 59.99
        };

        var result = validator.Validate(newProduct);

        await Assert.That(result.Errors)
                    .Contains(err => err.ErrorMessage == "Name is required.");
    }
}