// 작성자: Sangchan, Kim
// 작성일: 2025-03-27
// 기능: 회원 관련 비즈니스 로직을 처리하는 서비스
// 설명: 회원 CRUD 및 관련 기능을 제공합니다.
using Microsoft.Extensions.Logging;
using SMSAlarmSystem.Core.Interfaces;
using SMSAlarmSystem.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMSAlarmSystem.Services.Services
{
    public class MemberService
    {
        private readonly IMemberRepository _memberRepository;
        private readonly ILogger<MemberService> _logger;

        public MemberService(IMemberRepository memberRepository, ILogger<MemberService> logger)
        {
            _memberRepository = memberRepository ?? throw new ArgumentNullException(nameof(memberRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<Member>> GetAllMembersAsync()
        {
            try
            {
                _logger.LogInformation("모든 회원 조회 시작");
                return await _memberRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "모든 회원 조회 중 오류 발생");
                return new List<Member>();
            }
        }

        public async Task<Member?> GetMemberByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("회원 조회 시작: ID={Id}", id);
                return await _memberRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "회원 조회 중 오류 발생: ID={Id}", id);
                return null;
            }
        }

        public async Task<Member?> GetMemberByPhoneNumberAsync(string phoneNumber)
        {
            try
            {
                _logger.LogInformation("전화번호로 회원 조회 시작: PhoneNumber={PhoneNumber}", phoneNumber);
                return await _memberRepository.GetByPhoneNumberAsync(phoneNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "전화번호로 회원 조회 중 오류 발생: PhoneNumber={PhoneNumber}", phoneNumber);
                return null;
            }
        }

        public async Task<bool> AddMemberAsync(Member member)
        {
            if (member == null)
            {
                _logger.LogError("회원 추가 실패: 회원 객체가 null입니다.");
                return false;
            }

            try
            {
                _logger.LogInformation("회원 추가 시작: Name={Name}", member.Name);
                await _memberRepository.AddAsync(member);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "회원 추가 중 오류 발생: Name={Name}", member.Name);
                return false;
            }
        }

        public async Task<bool> UpdateMemberAsync(Member member)
        {
            if (member == null)
            {
                _logger.LogError("회원 업데이트 실패: 회원 객체가 null입니다.");
                return false;
            }

            try
            {
                _logger.LogInformation("회원 업데이트 시작: ID={Id}, Name={Name}", member.Id, member.Name);
                await _memberRepository.UpdateAsync(member);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "회원 업데이트 중 오류 발생: ID={Id}, Name={Name}", member.Id, member.Name);
                return false;
            }
        }

        public async Task<bool> DeleteMemberAsync(int id)
        {
            try
            {
                _logger.LogInformation("회원 삭제 시작: ID={Id}", id);
                var member = await _memberRepository.GetByIdAsync(id);
                if (member == null)
                {
                    _logger.LogWarning("회원 삭제 실패: 회원을 찾을 수 없음, ID={Id}", id);
                    return false;
                }
                await _memberRepository.DeleteAsync(member);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "회원 삭제 중 오류 발생: ID={Id}", id);
                return false;
            }
        }
    }
}
