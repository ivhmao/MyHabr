using MyHabr.Enums;
using MyHabr.Helpers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MyHabr.Entities
{
    public class Role
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        [Required, MaxLength(100)]
        public string Name { get; set; }
        [MaxLength(100)]
        public string? Description { get; set; }
        [JsonIgnore]
        public List<User> Users { get; set; } = new ();

        protected Role() { } //For EF
        private Role(RoleEnum @enum)
        {
            Id = (int)@enum;
            Name = @enum.ToString();
            Description = @enum.GetEnumDescription();
        }
        [JsonConstructorAttribute]
        public Role(string name) : this((int)Enum.Parse(typeof(RoleEnum), name))
        { }

        public Role(int id) : this((RoleEnum)id)
        { }

        public Role(int id, string name) : this((RoleEnum)id)
        { }

        public static implicit operator Role(RoleEnum @enum) => new Role(@enum);
        public static implicit operator RoleEnum(Role role) => (RoleEnum)role.Id;
    }
}
