// 작성자: Sangchan, Kim
// 작성일: 2025-03-31
// 기능: 메시지 그룹 관리 컨트롤러
// 설명: 메시지 그룹 목록 조회, 상세 보기, 추가, 수정, 삭제 및 회원 관리 기능을 제공하는 MVC 컨트롤러입니다.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using SMSAlarmSystem.Core.Models;
using SMSAlarmSystem.Models;
using SMSAlarmSystem.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace SMSAlarmSystem.Controllers
{
    /// <summary>
    /// 메시지 그룹 관리 기능을 제공하는 MVC 컨트롤러
    /// 메시지 그룹 목록 조회, 상세 보기, 추가, 수정, 삭제 및 회원 관리 기능을 제공합니다.
    /// </summary>
    public class MessageGroupsController : Controller
    {
        // 메시지 그룹 관련 비즈니스 로직을 처리하는 서비스
        private readonly IMessageGroupService _messageGroupService;

        // 회원 관련 비즈니스 로직을 처리하는 서비스
        private readonly IMemberService _memberService;

        // 로깅을 위한 로거 인스턴스
        private readonly ILogger<MessageGroupsController> _logger;

        /// <summary>
        /// MessageGroupsController 생성자
        /// </summary>
        /// <param name="messageGroupService">메시지 그룹 서비스 인스턴스</param>
        /// <param name="memberService">회원 서비스 인스턴스</param>
        /// <param name="logger">로깅을 위한 ILogger 인스턴스</param>
        /// <exception cref="ArgumentNullException">필수 매개변수가 null인 경우 발생</exception>
        public MessageGroupsController(
            IMessageGroupService messageGroupService,
            IMemberService memberService,
            ILogger<MessageGroupsController> logger)
        {
            _messageGroupService = messageGroupService ?? throw new ArgumentNullException(nameof(messageGroupService), "메시지 그룹 서비스는 null이 될 수 없습니다.");
            _memberService = memberService ?? throw new ArgumentNullException(nameof(memberService), "회원 서비스는 null이 될 수 없습니다.");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger), "로거는 null이 될 수 없습니다.");

            _logger.LogInformation("MessageGroupsController 초기화 완료");
        }

        /// <summary>
        /// 메시지 그룹 목록 페이지를 표시합니다.
        /// </summary>
        /// <param name="page">페이지 번호 (1부터 시작)</param>
        /// <param name="pageSize">페이지당 표시할 메시지 그룹 수</param>
        /// <returns>페이지네이션이 적용된 메시지 그룹 목록 뷰</returns>
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            try
            {
                // 페이지 번호와 페이지 크기 유효성 검사
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 10;
                if (pageSize > 100) pageSize = 100; // 최대 100개로 제한

                _logger.LogInformation("메시지 그룹 목록 조회 시작: 페이지={Page}, 페이지크기={PageSize}", page, pageSize);

                // 모든 메시지 그룹 조회
                var allGroups = await _messageGroupService.GetAllGroupsAsync();

                // null 체크 (방어적 프로그래밍)
                if (allGroups == null)
                {
                    _logger.LogWarning("메시지 그룹 서비스에서 null 반환됨. 빈 목록으로 대체합니다.");
                    allGroups = new List<MessageGroup>();
                }

                // 페이지네이션 적용
                var totalItems = allGroups.Count();
                var pagedGroups = allGroups
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // 페이지네이션 뷰 모델 생성
                var model = new PaginatedViewModel<MessageGroup>
                {
                    Items = pagedGroups,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = totalItems
                };

                _logger.LogInformation("메시지 그룹 목록 조회 완료: 총 {TotalItems}개 중 {PagedCount}개 표시",
                    totalItems, pagedGroups.Count);

                return View(model);
            }
            catch (Exception ex)
            {
                // 오류 로깅 추가 (CS0168 경고 해결)
                _logger.LogError(ex, "메시지 그룹 목록 조회 중 오류 발생: {ErrorMessage}", ex.Message);

                // 오류 발생 시 빈 모델로 뷰 표시
                TempData["ErrorMessage"] = "메시지 그룹 목록을 불러오는 중 오류가 발생했습니다.";
                return View(new PaginatedViewModel<MessageGroup>());
            }
        }


        /// <summary>
        /// 메시지 그룹 상세 정보 페이지를 표시합니다.
        /// </summary>
        /// <param name="id">조회할 메시지 그룹 ID</param>
        /// <returns>메시지 그룹 상세 뷰 또는 NotFound</returns>
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("잘못된 메시지 그룹 ID: {Id}", id);
                    return BadRequest("유효하지 않은 메시지 그룹 ID입니다.");
                }

                _logger.LogInformation("메시지 그룹 상세 조회 시작: ID={Id}", id);
                var group = await _messageGroupService.GetGroupByIdAsync(id);

                if (group == null)
                {
                    _logger.LogWarning("메시지 그룹을 찾을 수 없음: ID={Id}", id);
                    return NotFound($"ID가 {id}인 메시지 그룹을 찾을 수 없습니다.");
                }

                _logger.LogInformation("메시지 그룹 상세 조회 완료: ID={Id}, 이름={Name}", id, group.Name);
                return View(group);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "메시지 그룹 상세 조회 중 오류 발생: ID={Id}, 오류={ErrorMessage}", id, ex.Message);

                // 오류 메시지를 TempData에 저장하여 뷰에서 표시
                TempData["ErrorMessage"] = $"메시지 그룹 정보를 불러오는 중 오류가 발생했습니다: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 메시지 그룹 생성 페이지를 표시합니다.
        /// </summary>
        /// <returns>메시지 그룹 생성 뷰</returns>
        public IActionResult Create()
        {
            _logger.LogInformation("메시지 그룹 생성 페이지 요청");
            return View();
        }

        /// <summary>
        /// 메시지 그룹 생성을 처리합니다.
        /// </summary>
        /// <param name="group">생성할 메시지 그룹 정보</param>
        /// <returns>성공 시 메시지 그룹 목록 페이지로 리디렉션, 실패 시 생성 페이지 다시 표시</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MessageGroup group)
        {
            try
            {
                // null 체크
                if (group == null)
                {
                    _logger.LogWarning("메시지 그룹 생성 실패: 그룹 객체가 null입니다.");
                    ModelState.AddModelError(string.Empty, "메시지 그룹 정보가 제공되지 않았습니다.");
                    return View();
                }

                if (ModelState.IsValid)
                {
                    _logger.LogInformation("메시지 그룹 생성 시작: 이름={Name}", group.Name);

                    var result = await _messageGroupService.AddGroupAsync(group);
                    if (result)
                    {
                        _logger.LogInformation("메시지 그룹 생성 성공: ID={Id}, 이름={Name}", group.Id, group.Name);
                        TempData["SuccessMessage"] = "메시지 그룹이 성공적으로 생성되었습니다.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        _logger.LogWarning("메시지 그룹 생성 실패: 이름={Name}", group.Name);
                        ModelState.AddModelError(string.Empty, "메시지 그룹 생성에 실패했습니다. 다시 시도해 주세요.");
                    }
                }
                else
                {
                    _logger.LogWarning("메시지 그룹 생성 유효성 검사 실패");
                }

                return View(group);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "메시지 그룹 생성 중 오류 발생: 이름={Name}, 오류={ErrorMessage}",
                    group?.Name ?? "Unknown", ex.Message);

                ModelState.AddModelError(string.Empty, $"메시지 그룹 생성 중 오류가 발생했습니다: {ex.Message}");
                return View(group);
            }
        }

        /// <summary>
        /// 메시지 그룹 수정 페이지를 표시합니다.
        /// </summary>
        /// <param name="id">수정할 메시지 그룹 ID</param>
        /// <returns>메시지 그룹 수정 뷰 또는 NotFound</returns>
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("잘못된 메시지 그룹 ID: {Id}", id);
                    return BadRequest("유효하지 않은 메시지 그룹 ID입니다.");
                }

                _logger.LogInformation("메시지 그룹 수정 페이지 요청: ID={Id}", id);
                var group = await _messageGroupService.GetGroupByIdAsync(id);

                if (group == null)
                {
                    _logger.LogWarning("메시지 그룹을 찾을 수 없음: ID={Id}", id);
                    return NotFound($"ID가 {id}인 메시지 그룹을 찾을 수 없습니다.");
                }

                return View(group);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "메시지 그룹 수정 페이지 로드 중 오류 발생: ID={Id}, 오류={ErrorMessage}", id, ex.Message);

                TempData["ErrorMessage"] = $"메시지 그룹 정보를 불러오는 중 오류가 발생했습니다: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 메시지 그룹 수정을 처리합니다.
        /// </summary>
        /// <param name="id">수정할 메시지 그룹 ID</param>
        /// <param name="group">수정된 메시지 그룹 정보</param>
        /// <returns>성공 시 메시지 그룹 목록 페이지로 리디렉션, 실패 시 수정 페이지 다시 표시</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MessageGroup group)
        {
            try
            {
                // null 체크
                if (group == null)
                {
                    _logger.LogWarning("메시지 그룹 수정 실패: 그룹 객체가 null입니다.");
                    ModelState.AddModelError(string.Empty, "메시지 그룹 정보가 제공되지 않았습니다.");
                    return View();
                }

                // ID 일치 여부 확인
                if (id != group.Id)
                {
                    _logger.LogWarning("메시지 그룹 수정 실패: URL의 ID({UrlId})와 폼의 ID({FormId})가 일치하지 않습니다.", id, group.Id);
                    return NotFound($"ID가 일치하지 않습니다.");
                }

                if (ModelState.IsValid)
                {
                    _logger.LogInformation("메시지 그룹 수정 시작: ID={Id}, 이름={Name}", id, group.Name);

                    var result = await _messageGroupService.UpdateGroupAsync(group);
                    if (result)
                    {
                        _logger.LogInformation("메시지 그룹 수정 성공: ID={Id}, 이름={Name}", id, group.Name);
                        TempData["SuccessMessage"] = "메시지 그룹 정보가 성공적으로 수정되었습니다.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        _logger.LogWarning("메시지 그룹 수정 실패: ID={Id}, 이름={Name}", id, group.Name);
                        ModelState.AddModelError(string.Empty, "메시지 그룹 정보 수정에 실패했습니다. 다시 시도해 주세요.");
                    }
                }
                else
                {
                    _logger.LogWarning("메시지 그룹 수정 유효성 검사 실패: ID={Id}", id);
                }

                return View(group);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "메시지 그룹 수정 중 오류 발생: ID={Id}, 오류={ErrorMessage}", id, ex.Message);

                ModelState.AddModelError(string.Empty, $"메시지 그룹 정보 수정 중 오류가 발생했습니다: {ex.Message}");
                return View(group);
            }
        }

        /// <summary>
        /// 메시지 그룹 삭제 확인 페이지를 표시합니다.
        /// </summary>
        /// <param name="id">삭제할 메시지 그룹 ID</param>
        /// <returns>메시지 그룹 삭제 확인 뷰 또는 NotFound</returns>
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("잘못된 메시지 그룹 ID: {Id}", id);
                    return BadRequest("유효하지 않은 메시지 그룹 ID입니다.");
                }

                _logger.LogInformation("메시지 그룹 삭제 페이지 요청: ID={Id}", id);
                var group = await _messageGroupService.GetGroupByIdAsync(id);

                if (group == null)
                {
                    _logger.LogWarning("메시지 그룹을 찾을 수 없음: ID={Id}", id);
                    return NotFound($"ID가 {id}인 메시지 그룹을 찾을 수 없습니다.");
                }

                return View(group);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "메시지 그룹 삭제 페이지 로드 중 오류 발생: ID={Id}, 오류={ErrorMessage}", id, ex.Message);

                TempData["ErrorMessage"] = $"메시지 그룹 정보를 불러오는 중 오류가 발생했습니다: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 메시지 그룹 삭제를 처리합니다.
        /// </summary>
        /// <param name="id">삭제할 메시지 그룹 ID</param>
        /// <returns>메시지 그룹 목록 페이지로 리디렉션</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("잘못된 메시지 그룹 ID: {Id}", id);
                    TempData["ErrorMessage"] = "유효하지 않은 메시지 그룹 ID입니다.";
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogInformation("메시지 그룹 삭제 시작: ID={Id}", id);

                var result = await _messageGroupService.DeleteGroupAsync(id);
                if (result)
                {
                    _logger.LogInformation("메시지 그룹 삭제 성공: ID={Id}", id);
                    TempData["SuccessMessage"] = "메시지 그룹이 성공적으로 삭제되었습니다.";
                }
                else
                {
                    _logger.LogWarning("메시지 그룹 삭제 실패: ID={Id}", id);
                    TempData["ErrorMessage"] = "메시지 그룹 삭제에 실패했습니다. 다시 시도해 주세요.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "메시지 그룹 삭제 중 오류 발생: ID={Id}, 오류={ErrorMessage}", id, ex.Message);

                TempData["ErrorMessage"] = $"메시지 그룹 삭제 중 오류가 발생했습니다: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 그룹에 회원 추가 페이지를 표시합니다.
        /// </summary>
        /// <param name="id">회원을 추가할 그룹 ID</param>
        /// <returns>회원 추가 뷰 또는 NotFound</returns>
        public async Task<IActionResult> AddMember(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("잘못된 메시지 그룹 ID: {Id}", id);
                    return BadRequest("유효하지 않은 메시지 그룹 ID입니다.");
                }

                _logger.LogInformation("그룹에 회원 추가 페이지 요청: 그룹ID={GroupId}", id);
                var group = await _messageGroupService.GetGroupByIdAsync(id);

                if (group == null)
                {
                    _logger.LogWarning("메시지 그룹을 찾을 수 없음: ID={Id}", id);
                    return NotFound($"ID가 {id}인 메시지 그룹을 찾을 수 없습니다.");
                }

                try
                {
                    var members = await _memberService.GetAllMembersAsync();

                    // 이미 그룹에 속한 회원 제외 (선택적)
                    var groupMembers = await _messageGroupService.GetGroupMembersAsync(id);
                    var availableMembers = members.Where(m => !groupMembers.Any(gm => gm.Id == m.Id)).ToList();

                    ViewBag.MemberId = new SelectList(availableMembers, "Id", "Name");
                    ViewBag.GroupId = id;
                    ViewBag.GroupName = group.Name;

                    return View();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "그룹에 회원 추가 페이지 로드 중 오류 발생: 그룹ID={GroupId}, 오류={ErrorMessage}",
                        id, ex.Message);

                    TempData["ErrorMessage"] = $"회원 목록을 불러오는 중 오류가 발생했습니다: {ex.Message}";
                    return RedirectToAction(nameof(Details), new { id });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "그룹에 회원 추가 페이지 로드 중 오류 발생: ID={Id}, 오류={ErrorMessage}", id, ex.Message);

                TempData["ErrorMessage"] = $"메시지 그룹 정보를 불러오는 중 오류가 발생했습니다: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 그룹에 회원 추가를 처리합니다.
        /// </summary>
        /// <param name="groupId">회원을 추가할 그룹 ID</param>
        /// <param name="memberId">추가할 회원 ID</param>
        /// <returns>그룹 상세 페이지로 리디렉션</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMember(int groupId, int memberId)
        {
            try
            {
                // ID 유효성 검사
                if (groupId <= 0)
                {
                    _logger.LogWarning("그룹에 회원 추가 실패: 유효하지 않은 그룹 ID={GroupId}", groupId);
                    TempData["ErrorMessage"] = "유효하지 않은 그룹 ID입니다.";
                    return RedirectToAction(nameof(Index));
                }

                if (memberId <= 0)
                {
                    _logger.LogWarning("그룹에 회원 추가 실패: 유효하지 않은 회원 ID={MemberId}", memberId);
                    TempData["ErrorMessage"] = "유효하지 않은 회원 ID입니다.";
                    return RedirectToAction(nameof(Details), new { id = groupId });
                }

                _logger.LogInformation("그룹에 회원 추가 시작: 그룹ID={GroupId}, 회원ID={MemberId}", groupId, memberId);

                var result = await _messageGroupService.AddMemberToGroupAsync(groupId, memberId);
                if (result)
                {
                    _logger.LogInformation("그룹에 회원 추가 성공: 그룹ID={GroupId}, 회원ID={MemberId}", groupId, memberId);
                    TempData["SuccessMessage"] = "회원이 그룹에 성공적으로 추가되었습니다.";
                }
                else
                {
                    _logger.LogWarning("그룹에 회원 추가 실패: 그룹ID={GroupId}, 회원ID={MemberId}", groupId, memberId);
                    TempData["ErrorMessage"] = "회원을 그룹에 추가하는데 실패했습니다. 다시 시도해 주세요.";
                }

                return RedirectToAction(nameof(Details), new { id = groupId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "그룹에 회원 추가 중 오류 발생: 그룹ID={GroupId}, 회원ID={MemberId}, 오류={ErrorMessage}",
                    groupId, memberId, ex.Message);

                TempData["ErrorMessage"] = $"회원 추가 중 오류가 발생했습니다: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id = groupId });
            }
        }

        /// <summary>
        /// 그룹에서 회원 제거를 처리합니다.
        /// </summary>
        /// <param name="groupId">회원을 제거할 그룹 ID</param>
        /// <param name="memberId">제거할 회원 ID</param>
        /// <returns>그룹 상세 페이지로 리디렉션</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMember(int groupId, int memberId)
        {
            try
            {
                // ID 유효성 검사
                if (groupId <= 0)
                {
                    _logger.LogWarning("그룹에서 회원 제거 실패: 유효하지 않은 그룹 ID={GroupId}", groupId);
                    TempData["ErrorMessage"] = "유효하지 않은 그룹 ID입니다.";
                    return RedirectToAction(nameof(Index));
                }

                if (memberId <= 0)
                {
                    _logger.LogWarning("그룹에서 회원 제거 실패: 유효하지 않은 회원 ID={MemberId}", memberId);
                    TempData["ErrorMessage"] = "유효하지 않은 회원 ID입니다.";
                    return RedirectToAction(nameof(Details), new { id = groupId });
                }

                _logger.LogInformation("그룹에서 회원 제거 시작: 그룹ID={GroupId}, 회원ID={MemberId}", groupId, memberId);

                var result = await _messageGroupService.RemoveMemberFromGroupAsync(groupId, memberId);
                if (result)
                {
                    _logger.LogInformation("그룹에서 회원 제거 성공: 그룹ID={GroupId}, 회원ID={MemberId}", groupId, memberId);
                    TempData["SuccessMessage"] = "회원이 그룹에서 성공적으로 제거되었습니다.";
                }
                else
                {
                    _logger.LogWarning("그룹에서 회원 제거 실패: 그룹ID={GroupId}, 회원ID={MemberId}", groupId, memberId);
                    TempData["ErrorMessage"] = "회원을 그룹에서 제거하는데 실패했습니다. 다시 시도해 주세요.";
                }

                return RedirectToAction(nameof(Details), new { id = groupId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "그룹에서 회원 제거 중 오류 발생: 그룹ID={GroupId}, 회원ID={MemberId}, 오류={ErrorMessage}",
                    groupId, memberId, ex.Message);

                TempData["ErrorMessage"] = $"회원 제거 중 오류가 발생했습니다: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id = groupId });
            }
        }
    }
}

