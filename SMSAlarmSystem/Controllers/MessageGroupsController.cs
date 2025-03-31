using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SMSAlarmSystem.Core.Models;
using SMSAlarmSystem.Services.Interfaces;
using SMSAlarmSystem.Services.Services;
using System.Threading.Tasks;

namespace SMSAlarmSystem.Controllers
{
    public class MessageGroupsController : Controller
    {
        private readonly IMessageGroupService _messageGroupService;
        private readonly MemberService _memberService;

        public MessageGroupsController(IMessageGroupService messageGroupService, MemberService memberService)
        {
            _messageGroupService = messageGroupService;
            _memberService = memberService;
        }

        // 그룹 목록 페이지
        public async Task<IActionResult> Index()
        {
            var groups = await _messageGroupService.GetAllGroupsAsync();
            return View(groups);
        }

        // 그룹 상세 페이지
        public async Task<IActionResult> Details(int id)
        {
            var group = await _messageGroupService.GetGroupByIdAsync(id);
            if (group == null)
            {
                return NotFound();
            }
            return View(group);
        }

        // 그룹 생성 페이지 (GET)
        public IActionResult Create()
        {
            return View();
        }

        // 그룹 생성 처리 (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MessageGroup group)
        {
            if (ModelState.IsValid)
            {
                await _messageGroupService.AddGroupAsync(group);
                return RedirectToAction(nameof(Index));
            }
            return View(group);
        }

        // 그룹 수정 페이지 (GET)
        public async Task<IActionResult> Edit(int id)
        {
            var group = await _messageGroupService.GetGroupByIdAsync(id);
            if (group == null)
            {
                return NotFound();
            }
            return View(group);
        }

        // 그룹 수정 처리 (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MessageGroup group)
        {
            if (id != group.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _messageGroupService.UpdateGroupAsync(group);
                return RedirectToAction(nameof(Index));
            }
            return View(group);
        }

        // 그룹 삭제 페이지 (GET)
        public async Task<IActionResult> Delete(int id)
        {
            var group = await _messageGroupService.GetGroupByIdAsync(id);
            if (group == null)
            {
                return NotFound();
            }
            return View(group);
        }

        // 그룹 삭제 처리 (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _messageGroupService.DeleteGroupAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // 그룹에 회원 추가 페이지 (GET)
        public async Task<IActionResult> AddMember(int id)
        {
            var group = await _messageGroupService.GetGroupByIdAsync(id);
            if (group == null)
            {
                return NotFound();
            }

            var members = await _memberService.GetAllMembersAsync();
            ViewBag.MemberId = new SelectList(members, "Id", "Name");
            ViewBag.GroupId = id;
            ViewBag.GroupName = group.Name;

            return View();
        }

        // 그룹에 회원 추가 처리 (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMember(int groupId, int memberId)
        {
            await _messageGroupService.AddMemberToGroupAsync(groupId, memberId);
            return RedirectToAction(nameof(Details), new { id = groupId });
        }

        // 그룹에서 회원 제거
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMember(int groupId, int memberId)
        {
            await _messageGroupService.RemoveMemberFromGroupAsync(groupId, memberId);
            return RedirectToAction(nameof(Details), new { id = groupId });
        }
    }
}
