using Microsoft.AspNetCore.Mvc;
using SMSAlarmSystem.Core.Models;
using SMSAlarmSystem.Services.Services;
using System.Threading.Tasks;

namespace SMSAlarmSystem.Controllers
{
    public class MembersController : Controller
    {
        private readonly MemberService _memberService;

        public MembersController(MemberService memberService)
        {
            _memberService = memberService;
        }

        // 회원 목록 페이지
        public async Task<IActionResult> Index()
        {
            var members = await _memberService.GetAllMembersAsync();
            return View(members);
        }

        // 회원 상세 페이지
        public async Task<IActionResult> Details(int id)
        {
            var member = await _memberService.GetMemberByIdAsync(id);
            if (member == null)
            {
                return NotFound();
            }
            return View(member);
        }

        // 회원 생성 페이지 (GET)
        public IActionResult Create()
        {
            return View();
        }

        // 회원 생성 처리 (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Member member)
        {
            if (ModelState.IsValid)
            {
                await _memberService.AddMemberAsync(member);
                return RedirectToAction(nameof(Index));
            }
            return View(member);
        }

        // 회원 수정 페이지 (GET)
        public async Task<IActionResult> Edit(int id)
        {
            var member = await _memberService.GetMemberByIdAsync(id);
            if (member == null)
            {
                return NotFound();
            }
            return View(member);
        }

        // 회원 수정 처리 (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Member member)
        {
            if (id != member.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _memberService.UpdateMemberAsync(member);
                return RedirectToAction(nameof(Index));
            }
            return View(member);
        }

        // 회원 삭제 페이지 (GET)
        public async Task<IActionResult> Delete(int id)
        {
            var member = await _memberService.GetMemberByIdAsync(id);
            if (member == null)
            {
                return NotFound();
            }
            return View(member);
        }

        // 회원 삭제 처리 (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _memberService.DeleteMemberAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
