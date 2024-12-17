import { useState, useEffect } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import '../css/Sidebar.css';
// import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
// import { faPlus, faMessage } from '@fortawesome/free-solid-svg-icons';
// import { faMagnifyingGlass } from '@fortawesome/free-solid-svg-icons';

const Sidebar = ({ topics }: { topics: any[] }) => {
    const navigate = useNavigate();
    const location = useLocation();
    const [selectedTopicId, setSelectedTopicId] = useState<string | null>(null);
    const [isCreatingTopic, setIsCreatingTopic] = useState(false);
    const [searchTerm, setSearchTerm] = useState('');

    const handleTopicClick = (topic: any) => {
        setSelectedTopicId(topic.id);
        setIsCreatingTopic(false);
        navigate(`/topic/${topic.id}`);
    };

    const handleCreateTopicClick = () => {
        setSelectedTopicId(null);
        setIsCreatingTopic(true);
        navigate('/create-topic');
    };

    useEffect(() => {
        const pathParts = location.pathname.split('/');
        if (pathParts[1] === 'topic' && pathParts[2]) {
            setSelectedTopicId(pathParts[2]);
        } else {
            setSelectedTopicId(null);
        }
    }, [location.pathname]);

    const handleSearch = (event: React.ChangeEvent<HTMLInputElement>) => {
        setSearchTerm(event.target.value);
    };

    const filteredTopics = topics.filter((topic: any) =>
        topic.name.toLowerCase().includes(searchTerm.toLowerCase())
    );

    return (
        <div className='sidebar-container'>
            <h3>Topics</h3>
            <div className='topic-search-container'>
                {/* <FontAwesomeIcon className='topic-search-icon' icon={faMagnifyingGlass} /> */}
                <input
                    type='text'
                    placeholder='Search topics...'
                    value={searchTerm}
                    onChange={handleSearch}
                    className='search-bar'
                />
            </div>
            <div
                className={`topic ${isCreatingTopic ? 'selected' : ''}`}
                onClick={handleCreateTopicClick}
            >
                <div className='sidebar-item'>
                    {/* <FontAwesomeIcon className='icon' icon={faPlus} /> */}
                    <p>Create a topic</p>
                </div>
            </div>
            {filteredTopics.map((topic: any) => (
                <li key={topic.id}>
                    <div
                        className={`topic ${selectedTopicId === topic.id ? 'selected' : ''} sidebar-item`}
                        onClick={() => handleTopicClick(topic)}
                    >
                        {/* <FontAwesomeIcon className='icon' icon={faMessage} /> */}
                        <p>{topic.name}</p>
                    </div>
                </li>
            ))}
        </div>
    );
};

export default Sidebar;