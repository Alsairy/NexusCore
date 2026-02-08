import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { of } from 'rxjs';

import { DelegationsComponent } from './delegations.component';
import { DelegationService } from '../services/delegation.service';
import { createMockDelegationService, setupDefaultMockReturns } from '../../testing/mock-services';
import { createMockDelegation } from '../../testing/saudi-test-helpers';

describe('DelegationsComponent', () => {
  let component: DelegationsComponent;
  let fixture: ComponentFixture<DelegationsComponent>;
  let mockDelegationService: jasmine.SpyObj<DelegationService>;

  beforeEach(async () => {
    mockDelegationService = createMockDelegationService() as jasmine.SpyObj<DelegationService>;
    setupDefaultMockReturns({ delegationService: mockDelegationService });

    await TestBed.configureTestingModule({
      imports: [DelegationsComponent, FormsModule],
      schemas: [NO_ERRORS_SCHEMA],
    })
      .overrideComponent(DelegationsComponent, {
        set: {
          providers: [{ provide: DelegationService, useValue: mockDelegationService }],
        },
      })
      .compileComponents();

    fixture = TestBed.createComponent(DelegationsComponent);
    component = fixture.componentInstance;
  });

  it('should create the component', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should load delegations on init', () => {
    const mockDel = createMockDelegation();
    mockDelegationService.getList.and.returnValue(of({ items: [mockDel], totalCount: 1 }));

    fixture.detectChanges();

    expect(mockDelegationService.getList).toHaveBeenCalled();
    expect(component.delegations.length).toBe(1);
  });

  it('should set default dates on init', () => {
    fixture.detectChanges();

    const today = new Date();
    const expectedStart = today.toISOString().split('T')[0];

    expect(component.newDelegation.startDate).toBe(expectedStart);
    expect(component.newDelegation.endDate).toBeTruthy();

    // Verify end date is approximately 7 days after start
    const startDate = new Date(component.newDelegation.startDate);
    const endDate = new Date(component.newDelegation.endDate);
    const diffDays = Math.round((endDate.getTime() - startDate.getTime()) / (1000 * 60 * 60 * 24));
    expect(diffDays).toBe(7);
  });

  it('should toggle form visibility', () => {
    fixture.detectChanges();

    expect(component.showForm).toBeFalse();

    component.toggleForm();
    expect(component.showForm).toBeTrue();

    component.toggleForm();
    expect(component.showForm).toBeFalse();
  });

  it('should call create on save for new delegation', () => {
    const mockDel = createMockDelegation();
    mockDelegationService.create.and.returnValue(of(mockDel));

    fixture.detectChanges();

    component.isEditMode = false;
    component.newDelegation = {
      delegateUserId: 'user-002',
      startDate: '2024-06-15',
      endDate: '2024-06-22',
      reason: 'On vacation',
    };

    component.saveDelegation();

    expect(mockDelegationService.create).toHaveBeenCalledWith({
      delegateUserId: 'user-002',
      startDate: '2024-06-15',
      endDate: '2024-06-22',
      reason: 'On vacation',
    });
  });

  it('should call update on save for edit', () => {
    const mockDel = createMockDelegation();
    mockDelegationService.update.and.returnValue(of(mockDel));

    fixture.detectChanges();

    component.isEditMode = true;
    component.editingId = 'del-001';
    component.newDelegation = {
      delegateUserId: 'user-003',
      startDate: '2024-07-01',
      endDate: '2024-07-08',
      reason: 'Updated reason',
    };

    component.saveDelegation();

    expect(mockDelegationService.update).toHaveBeenCalledWith('del-001', {
      delegateUserId: 'user-003',
      startDate: '2024-07-01',
      endDate: '2024-07-08',
      reason: 'Updated reason',
    });
  });

  it('should call delete and reload', fakeAsync(() => {
    mockDelegationService.delete.and.returnValue(of(void 0));
    mockDelegationService.getList.and.returnValue(of({ items: [], totalCount: 0 }));

    fixture.detectChanges();
    mockDelegationService.getList.calls.reset();

    component.deleteDelegation('del-001');
    tick();

    expect(mockDelegationService.delete).toHaveBeenCalledWith('del-001');
    expect(mockDelegationService.getList).toHaveBeenCalled();
  }));

  it('should populate form on edit', () => {
    fixture.detectChanges();

    const delegation = createMockDelegation({
      id: 'del-001',
      delegateUserId: 'user-005',
      startDate: '2024-08-01T00:00:00Z',
      endDate: '2024-08-15T00:00:00Z',
      reason: 'Business trip',
    });

    component.editDelegation(delegation);

    expect(component.isEditMode).toBeTrue();
    expect(component.editingId).toBe('del-001');
    expect(component.showForm).toBeTrue();
    expect(component.newDelegation.delegateUserId).toBe('user-005');
    expect(component.newDelegation.startDate).toBe('2024-08-01');
    expect(component.newDelegation.endDate).toBe('2024-08-15');
    expect(component.newDelegation.reason).toBe('Business trip');
  });

  it('should show empty message when no delegations', () => {
    mockDelegationService.getList.and.returnValue(of({ items: [], totalCount: 0 }));

    fixture.detectChanges();

    expect(component.delegations.length).toBe(0);
    expect(component.loading).toBeFalse();
  });
});
