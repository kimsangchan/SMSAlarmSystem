// 작성자: Sangchan, Kim
// 작성일: 2025-03-31
// 기능: 회원 데이터를 관리하는 리포지토리 클래스
// 설명: 이 클래스는 데이터베이스와 상호작용하여 회원 데이터를 CRUD 방식으로 관리합니다.

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SMSAlarmSystem.Core.Interfaces;
using SMSAlarmSystem.Core.Models;
using SMSAlarmSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMSAlarmSystem.Infrastructure.Repositories
{
    /// <summary>
    /// 회원 데이터를 관리하는 리포지토리 클래스
    /// </summary>
    public class MemberRepository : Repository<Member>, IMemberRepository
    {
        // 데이터베이스 컨텍스트 (부모 클래스의 필드를 가리기 위해 new 키워드 사용)
        private new readonly ApplicationDbContext _context;

        // 로거 인스턴스 (부모 클래스의 필드를 가리기 위해 new 키워드 사용)
        private new readonly ILogger<MemberRepository> _logger;

        /// <summary>
        /// MemberRepository 생성자
        /// </summary>
        /// <param name="context">데이터베이스 컨텍스트</param>
        /// <param name="logger">로깅을 위한 ILogger 인스턴스</param>
        /// <exception cref="ArgumentNullException">필수 매개변수가 null인 경우 발생</exception>
        public MemberRepository(ApplicationDbContext context, ILogger<MemberRepository> logger)
            : base(context, logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context), "데이터베이스 컨텍스트는 null이 될 수 없습니다.");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger), "로거는 null이 될 수 없습니다.");
        }

        /// <summary>
        /// 전화번호로 회원을 조회합니다.
        /// </summary>
        /// <param name="phoneNumber">조회할 회원의 전화번호</param>
        /// <returns>조회된 회원 또는 null(회원이 없거나 오류 발생 시)</returns>
        public async Task<Member?> GetByPhoneNumberAsync(string phoneNumber)
        {
            // 전화번호 유효성 검사
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                _logger.LogWarning("전화번호로 회원 조회 실패: 전화번호가 null 또는 빈 문자열입니다.");
                return null;
            }

            try
            {
                _logger.LogInformation("전화번호로 회원 조회 시작: PhoneNumber={PhoneNumber}", phoneNumber);
                var member = await _context.Members.FirstOrDefaultAsync(m => m.PhoneNumber == phoneNumber);

                if (member == null)
                {
                    _logger.LogWarning("전화번호로 회원을 찾을 수 없음: PhoneNumber={PhoneNumber}", phoneNumber);
                }
                else
                {
                    _logger.LogInformation("전화번호로 회원 조회 완료: PhoneNumber={PhoneNumber}, 이름={Name}",
                        phoneNumber, member.Name);
                }

                return member;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "전화번호로 회원 조회 중 오류 발생: PhoneNumber={PhoneNumber}, 오류={ErrorMessage}",
                    phoneNumber, ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 특정 그룹에 속한 회원 목록을 조회합니다.
        /// </summary>
        /// <param name="groupId">조회할 그룹 ID</param>
        /// <returns>해당 그룹에 속한 회원 목록</returns>
        /// <exception cref="ArgumentException">유효하지 않은 그룹 ID인 경우 발생</exception>
        public async Task<IEnumerable<Member>> GetMembersByGroupIdAsync(int groupId)
        {
            // ID 유효성 검사
            if (groupId <= 0)
            {
                _logger.LogWarning("그룹 ID로 회원 목록 조회 실패: 유효하지 않은 그룹 ID={GroupId}", groupId);
                return new List<Member>();
            }

            try
            {
                _logger.LogInformation("그룹 ID로 회원 목록 조회 시작: GroupID={GroupId}", groupId);

                var members = await _context.MessageGroupMembers
                    .Where(mgm => mgm.MessageGroupId == groupId)
                    .Select(mgm => mgm.Member)
                    .ToListAsync();

                _logger.LogInformation("그룹 ID로 회원 목록 조회 완료: GroupID={GroupId}, 회원 수={Count}",
                    groupId, members.Count);

                return members;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "그룹 ID로 회원 목록 조회 중 오류 발생: GroupID={GroupId}, 오류={ErrorMessage}",
                    groupId, ex.Message);
                throw; // 서비스 계층에서 예외 처리를 위해 예외를 다시 던짐
            }
        }
    }
}
