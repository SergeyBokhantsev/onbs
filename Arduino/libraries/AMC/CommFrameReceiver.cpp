
#include "CommFrameReceiver.h"

#define LOOKING_FOR_FRAME_BEGIN 0
#define LOOKING_FOR_FRAME_TYPE 1
#define LOOKING_FOR_FRAME_ID1 2
#define LOOKING_FOR_FRAME_ID2 3
#define LOOKING_FOR_FRAME_CHECKSUM 4
#define LOOKING_FOR_FRAME_END 5

CommFrameReceiver::CommFrameReceiver(HardwareSerial* _inputPort) : CommFrameProcessor(),
inputPort(_inputPort),
frame_type(0),
frame_id(0),
frame_checksum(0),
frame_len(0),
matchedFrameMarkCount(0),
readingIncomingDataState(LOOKING_FOR_FRAME_BEGIN)
{
}

bool CommFrameReceiver::get_frame(char** out_frame_array, int* out_frame_len, char* out_frame_type, unsigned short* out_frame_id)
{
	while(inputPort->available())
	{
		char value = (char)inputPort->read();
		
		switch (readingIncomingDataState)
		{
			case LOOKING_FOR_FRAME_BEGIN:
				repeat:
				if (comm_frame_begin[matchedFrameMarkCount] == value)
				{
                    matchedFrameMarkCount++;
                }
                else if (matchedFrameMarkCount > 0)
                {
                    matchedFrameMarkCount = 0;
					goto repeat;
                }
			
				if (matchedFrameMarkCount == COMM_FRAME_BEGIN_LEN)
				{
					readingIncomingDataState = LOOKING_FOR_FRAME_TYPE;
                    matchedFrameMarkCount = 0;
				}
				break;
				
			case LOOKING_FOR_FRAME_TYPE:
				frame_type = value;
				readingIncomingDataState = LOOKING_FOR_FRAME_ID1;
				break;
				
			case LOOKING_FOR_FRAME_ID1:
				frame_id = ((unsigned short)value) << 8;
				readingIncomingDataState = LOOKING_FOR_FRAME_ID2;
				break;
				
			case LOOKING_FOR_FRAME_ID2:
				frame_id += value;
				readingIncomingDataState = LOOKING_FOR_FRAME_CHECKSUM;
				break;
				
			case LOOKING_FOR_FRAME_CHECKSUM:
				frame_checksum = value;
				readingIncomingDataState = LOOKING_FOR_FRAME_END;
				break;
				
			case LOOKING_FOR_FRAME_END:
				frame_data[frame_len++] = value;
				
				if (frame_len >= COMM_FRAME_END_LEN)
				{
                    int endMatchesCount = 0;
                                        
					for (int i=0; i<COMM_FRAME_END_LEN; ++i)
					{
						if (frame_data[frame_len - COMM_FRAME_END_LEN + i] == comm_frame_end[i])
						{
							endMatchesCount++;
						}
					}

                    if (endMatchesCount == COMM_FRAME_END_LEN)
                    {
						frame_len -= COMM_FRAME_END_LEN;
						
						*out_frame_array = frame_data;
						*out_frame_len = frame_len;
						*out_frame_type = frame_type;
						*out_frame_id = frame_id;

						bool frame_valid = frame_checksum == calculate_checksum(frame_data, frame_len);
						
						readingIncomingDataState = LOOKING_FOR_FRAME_BEGIN;
					    frame_len = 0;

					    return frame_valid;
                    }
				}
				
				if (frame_len == FRAME_DATA_SIZE)
				{
					readingIncomingDataState = LOOKING_FOR_FRAME_BEGIN;
					frame_len = 0;
				}
				
				break;
		}
	}

	return false;
}