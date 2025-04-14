# Nimbra Orchestration Regression Tests

## Regression Tests

Following use cases are currently automatically tested:

### RT_Booking_Life_Cycle

The Booking life cycle evaluates the creation of a booking based on a work order send by ScheduAll.

| Test Name | Description |
|--|--|
|Validate_Acknowledgment|Validates that when ScheduAll sends a work order via Smart Serial, the ScheduAll Generic Interop Manager element receives it and sends an acknowledgment (ACK) back through Smart Serial. If the ACK is not sent, ScheduAll will resend the work order every minute. This test ensures that an ACK is sent immediately upon receiving|
|Validate_WorkOrder|Ensures that upon receiving a work order from ScheduAll, an entry is correctly created in the Work Order table of the Generic Interop Manager element, containing all relevant work order information.|
|Validate_Booking|Confirms that an SRM booking is created based on the details provided in the received work order.|
