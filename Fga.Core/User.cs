namespace Fga;

public record User
{
    public record UserId(string Id) : User
    {
        public override string ToString()
        {
            return $"{Id}";
        }
    }

    public record UserSet(RelationObject Object, string Relation) : User
    {
        public override string ToString()
        {
            return $"{Object}#{Relation}";
        }

        public static User Parse(string userString)
        {
            var s = userString.Split('#');
            return new UserSet(RelationObject.Parse(s[0]), s[1]);
        }
    }
    
    public static User Parse(string userString)
    {
        return userString.Contains('#') ? UserSet.Parse(userString) : new UserId(userString);
    }
}