namespace Scheds.Application.Interfaces.Services
{
    public interface IEmptyRoomsService
    {
        public Task<List<string>> GetEmptyRooms(string dayOfWeek, string time);
    }
}
