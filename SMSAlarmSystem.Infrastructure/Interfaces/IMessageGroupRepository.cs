using SMSAlarmSystem.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMSAlarmSystem.Core.Interfaces
{
    // 메시지 그룹 저장소 인터페이스
    public interface IMessageGroupRepository : IRepository<MessageGroup>
    {
        // 활성화된 그룹만 가져오기
        Task<IEnumerable<MessageGroup>> GetActiveGroupsAsync();

        // 회원을 그룹에 추가
        Task AddMemberToGroupAsync(int groupId, int memberId);

        // 회원을 그룹에서 제거
        Task RemoveMemberFromGroupAsync(int groupId, int memberId);
    }
}
