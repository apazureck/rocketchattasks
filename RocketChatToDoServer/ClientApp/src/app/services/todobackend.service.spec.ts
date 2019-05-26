import { TestBed, inject } from '@angular/core/testing';

import { TodobackendService } from './todobackend.service';

describe('TodobackendService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [TodobackendService]
    });
  });

  it('should be created', inject([TodobackendService], (service: TodobackendService) => {
    expect(service).toBeTruthy();
  }));
});
