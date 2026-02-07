import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { of } from 'rxjs';

import { ApprovalInboxComponent } from './approval-inbox.component';
import { ApprovalService, ApprovalStatus } from '../services/approval.service';
import { createMockApprovalService, setupDefaultMockReturns } from '../../testing/mock-services';
import { createMockApprovalTask } from '../../testing/saudi-test-helpers';

describe('ApprovalInboxComponent', () => {
  let component: ApprovalInboxComponent;
  let fixture: ComponentFixture<ApprovalInboxComponent>;
  let mockApprovalService: jasmine.SpyObj<ApprovalService>;

  beforeEach(async () => {
    mockApprovalService = createMockApprovalService() as jasmine.SpyObj<ApprovalService>;
    setupDefaultMockReturns({ approvalService: mockApprovalService });

    await TestBed.configureTestingModule({
      imports: [ApprovalInboxComponent, FormsModule],
      schemas: [NO_ERRORS_SCHEMA],
    })
      .overrideComponent(ApprovalInboxComponent, {
        set: {
          providers: [{ provide: ApprovalService, useValue: mockApprovalService }],
        },
      })
      .compileComponents();

    fixture = TestBed.createComponent(ApprovalInboxComponent);
    component = fixture.componentInstance;
  });

  it('should create the component', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should load tasks on init', () => {
    const mockTask = createMockApprovalTask();
    mockApprovalService.getMyTasks.and.returnValue(of({ items: [mockTask], totalCount: 1 }));

    fixture.detectChanges();

    expect(mockApprovalService.getMyTasks).toHaveBeenCalled();
    expect(component.tasks.length).toBe(1);
    expect(component.totalCount).toBe(1);
  });

  it('should show loading spinner while loading', () => {
    fixture.detectChanges();

    component.loading = true;
    fixture.detectChanges();

    expect(component.loading).toBeTrue();
  });

  it('should display tasks in table', () => {
    const tasks = [
      createMockApprovalTask({ id: 'task-1', taskName: 'Approve Invoice 1' }),
      createMockApprovalTask({ id: 'task-2', taskName: 'Approve Invoice 2' }),
    ];
    mockApprovalService.getMyTasks.and.returnValue(of({ items: tasks, totalCount: 2 }));

    fixture.detectChanges();

    expect(component.tasks.length).toBe(2);
    expect(component.tasks[0].taskName).toBe('Approve Invoice 1');
    expect(component.tasks[1].taskName).toBe('Approve Invoice 2');
  });

  it('should show empty message when no tasks', () => {
    mockApprovalService.getMyTasks.and.returnValue(of({ items: [], totalCount: 0 }));

    fixture.detectChanges();

    expect(component.tasks.length).toBe(0);
    expect(component.loading).toBeFalse();
  });

  it('should toggle comment row', () => {
    const task = createMockApprovalTask({ id: 'task-1' });
    fixture.detectChanges();

    expect(component.expandedTaskId).toBeNull();

    component.toggleComment(task);
    expect(component.expandedTaskId).toBe('task-1');
    expect(component.actionComment).toBe('');

    component.toggleComment(task);
    expect(component.expandedTaskId).toBeNull();
  });

  it('should call approve with taskId and comment', () => {
    const task = createMockApprovalTask({ id: 'task-1' });
    const approvedTask = createMockApprovalTask({ id: 'task-1', status: ApprovalStatus.Approved });
    mockApprovalService.approve.and.returnValue(of(approvedTask));

    fixture.detectChanges();

    component.expandedTaskId = task.id;
    component.actionComment = 'Looks good';
    component.approveTask(task);

    expect(mockApprovalService.approve).toHaveBeenCalledWith({
      taskId: 'task-1',
      comment: 'Looks good',
    });
  });

  it('should call reject with taskId and comment', () => {
    const task = createMockApprovalTask({ id: 'task-1' });
    const rejectedTask = createMockApprovalTask({ id: 'task-1', status: ApprovalStatus.Rejected });
    mockApprovalService.reject.and.returnValue(of(rejectedTask));

    fixture.detectChanges();

    component.expandedTaskId = task.id;
    component.actionComment = 'Not approved';
    component.rejectTask(task);

    expect(mockApprovalService.reject).toHaveBeenCalledWith({
      taskId: 'task-1',
      comment: 'Not approved',
    });
  });

  it('should reload tasks after approve', fakeAsync(() => {
    const task = createMockApprovalTask({ id: 'task-1' });
    const approvedTask = createMockApprovalTask({ id: 'task-1', status: ApprovalStatus.Approved });
    mockApprovalService.approve.and.returnValue(of(approvedTask));
    mockApprovalService.getMyTasks.and.returnValue(of({ items: [], totalCount: 0 }));

    fixture.detectChanges();
    mockApprovalService.getMyTasks.calls.reset();

    component.actionComment = 'Approved';
    component.approveTask(task);
    tick();

    expect(mockApprovalService.getMyTasks).toHaveBeenCalled();
  }));

  it('should return correct badge classes', () => {
    fixture.detectChanges();

    expect(component.getStatusBadgeClass(ApprovalStatus.Pending)).toBe('bg-warning');
    expect(component.getStatusBadgeClass(ApprovalStatus.Approved)).toBe('bg-success');
    expect(component.getStatusBadgeClass(ApprovalStatus.Rejected)).toBe('bg-danger');
    expect(component.getStatusBadgeClass(ApprovalStatus.Escalated)).toBe('bg-info');
    expect(component.getStatusBadgeClass(ApprovalStatus.Delegated)).toBe('bg-secondary');
  });
});
