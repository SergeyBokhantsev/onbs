
#include "CommFrameReceiver.h"

#define LOOKING_FOR_FRAME_BEGIN 0
#define LOOKING_FOR_FRAME_TYPE 1
#define LOOKING_FOR_FRAME_END 2

CommFrameReceiver::CommFrameReceiver(HardwareSerial* _inputPort) : CommFrameProcessor(),
inputPort(_inputPort),
frame_type(0),
frame_len(0),
matchedFrameMarkCount(0),
readingIncomingDataState(LOOKING_FOR_FRAME_BEGIN)
{
}

bool CommFrameReceiver::get_frame(char** out_frame_array, int* out_frame_len, char* out_frame_type)
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
						*out_frame_array = frame_data;
						*out_frame_len = (frame_len - COMM_FRAME_END_LEN);
						*out_frame_type = frame_type;

						readingIncomingDataState = LOOKING_FOR_FRAME_BEGIN;
					    frame_len = 0;

					    return true;
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