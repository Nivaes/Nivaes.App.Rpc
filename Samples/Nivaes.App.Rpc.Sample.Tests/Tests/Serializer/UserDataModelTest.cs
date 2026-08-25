using MemoryPack;
using Nivaes.App.RPC.Sample;
using Nivaes.DataTestGenerator;

namespace Nivaes.App.Rpc.Test.Serializer
{
    public class UserDataModelTest
    {
        [Fact]
        public void SerializerDataModel()
        {
            var contact = ContactGenerator.GenerateContact();

            var user = new UserDataModel
            {
                IdUser = Guid.NewGuid(),
                Identification = $"ID00000",
                Name = contact.SortName,
                GivenName = contact.GivenName,
                FamilyName = contact.FamilyName,
                Email = contact.Email,
                PhoneNumber = contact.TelephoneNumber,
                TimeStamp = DateTime.UtcNow
            };

            var bin = MemoryPackSerializer.Serialize(user);
            var userCopy = MemoryPackSerializer.Deserialize<UserDataModel>(bin);

            userCopy.ShouldNotBeNull();
            userCopy.Id.ShouldBe(user.Id);
            userCopy.IdUser.ShouldBe(user.IdUser);

            userCopy.IdUser.ShouldBe(user.IdUser);
            userCopy.Identification.ShouldBe(user.Identification);
            userCopy.Name.ShouldBe(user.Name);
            userCopy.GivenName.ShouldBe(user.GivenName);
            userCopy.FamilyName.ShouldBe(user.FamilyName);
            userCopy.Email.ShouldBe(user.Email);
            userCopy.IdUser.ShouldBe(user.IdUser);
            userCopy.PhoneNumber.ShouldBe(user.PhoneNumber);
            userCopy.TimeStamp.ShouldBe(user.TimeStamp);
            userCopy.TimeStampTicks.ShouldBe(user.TimeStampTicks);
        }

        [Fact]
        public void SerializerDataModelOverwrite()
        {
            var contact = ContactGenerator.GenerateContact();

            var user = new UserDataModel
            {
                IdUser = Guid.NewGuid(),
                Identification = $"ID00000",
                Name = contact.SortName,
                GivenName = contact.GivenName,
                FamilyName = contact.FamilyName,
                Email = contact.Email,
                PhoneNumber = contact.TelephoneNumber,
                TimeStamp = DateTime.UtcNow
            };

            UserDataModel? userCopy = null;
            var bin = MemoryPackSerializer.Serialize(user);
            var i = MemoryPackSerializer.Deserialize(bin, ref userCopy);

            userCopy.ShouldNotBeNull();
            userCopy.Id.ShouldBe(user.Id);
            userCopy.IdUser.ShouldBe(user.IdUser);

            userCopy.IdUser.ShouldBe(user.IdUser);
            userCopy.Identification.ShouldBe(user.Identification);
            userCopy.Name.ShouldBe(user.Name);
            userCopy.GivenName.ShouldBe(user.GivenName);
            userCopy.FamilyName.ShouldBe(user.FamilyName);
            userCopy.Email.ShouldBe(user.Email);
            userCopy.IdUser.ShouldBe(user.IdUser);
            userCopy.PhoneNumber.ShouldBe(user.PhoneNumber);
            userCopy.TimeStamp.ShouldBe(user.TimeStamp);
            userCopy.TimeStampTicks.ShouldBe(user.TimeStampTicks);
        }

        [Fact]
        public async Task SerializerDataModelAsync()
        {
            var contact = ContactGenerator.GenerateContact();

            var user = new UserDataModel
            {
                IdUser = Guid.NewGuid(),
                Identification = $"ID00000",
                Name = contact.SortName,
                GivenName = contact.GivenName,
                FamilyName = contact.FamilyName,
                Email = contact.Email,
                PhoneNumber = contact.TelephoneNumber,
                TimeStamp = DateTime.UtcNow
            };

            MemoryStream ms = new MemoryStream();
            await MemoryPackSerializer.SerializeAsync(ms, user);
            ms.Position = 0;
            var userCopy = await MemoryPackSerializer.DeserializeAsync<UserDataModel>(ms);

            userCopy.ShouldNotBeNull();
            userCopy.Id.ShouldBe(user.Id);
            userCopy.IdUser.ShouldBe(user.IdUser);

            userCopy.IdUser.ShouldBe(user.IdUser);
            userCopy.Identification.ShouldBe(user.Identification);
            userCopy.Name.ShouldBe(user.Name);
            userCopy.GivenName.ShouldBe(user.GivenName);
            userCopy.FamilyName.ShouldBe(user.FamilyName);
            userCopy.Email.ShouldBe(user.Email);
            userCopy.IdUser.ShouldBe(user.IdUser);
            userCopy.PhoneNumber.ShouldBe(user.PhoneNumber);
            userCopy.TimeStamp.ShouldBe(user.TimeStamp);
            userCopy.TimeStampTicks.ShouldBe(user.TimeStampTicks);
        }
    }
}
