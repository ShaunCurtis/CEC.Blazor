
namespace CEC.Blazor.Data
{
    /// <summary>
    /// Base Data Record
    /// </summary>
    public class DbBaseRecord :IDbRecord<DbBaseRecord>
    {

        public int ID { get; set; } = -1;

        public string DisplayName { get; set; }
        public void SetNew() => this.ID = 0;

        public DbBaseRecord ShadowCopy()
        {
            return new DbBaseRecord() {
                ID = this.ID,
                DisplayName = this.DisplayName
            };
        }
    }
}
