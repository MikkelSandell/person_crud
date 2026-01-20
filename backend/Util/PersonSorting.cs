    using CrudApp.Backend.Models;
using MongoDB.Driver;

namespace CrudApp.Backend.Util
{
    public static class PersonSorting
    {
        /// <summary>
        /// Gets the appropriate sort definition based on the sortBy and sortOrder parameters.
        /// </summary>
        /// <param name="sortBy">The field to sort by (username, cpr, or profilepicture)</param>
        /// <param name="sortOrder">The sort order (asc or desc)</param>
        /// <returns>A SortDefinition for MongoDB</returns>
        public static SortDefinition<Person> GetSortDefinition(string sortBy, string sortOrder)
        {
            string sortField = sortBy.ToLower() switch
            {
                "cpr" => nameof(Person.Cpr),
                "profilepicture" => nameof(Person.ProfilePicture),
                "starsign" => nameof(Person.StarSign),
                _ => nameof(Person.Username)
            };

            var sortDef = sortOrder.ToLower() == "desc"
                ? Builders<Person>.Sort.Descending(sortField)
                : Builders<Person>.Sort.Ascending(sortField);

            return sortDef;
        }

        /// <summary>
        /// Gets the collation settings for case-insensitive sorting.
        /// </summary>
        /// <returns>A Collation object configured for English case-insensitive sorting</returns>
        public static Collation GetCollation()
        {
            return new Collation("en", strength: CollationStrength.Primary, caseLevel: false, numericOrdering: false);
        }

        /// <summary>
        /// Gets the FindOptions with collation for MongoDB queries.
        /// </summary>
        /// <returns>FindOptions configured with collation</returns>
        public static FindOptions GetFindOptions()
        {
            return new FindOptions { Collation = GetCollation() };
        }
    }
}
